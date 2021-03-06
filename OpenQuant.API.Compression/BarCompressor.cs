using SmartQuant.Data;
using System;
namespace OpenQuant.API.Compression
{
	internal abstract class BarCompressor
	{
		protected long oldBarSize;
		protected long newBarSize;
		protected global::OpenQuant.API.Bar bar;
		private event CompressedBarEventHandler NewCompressedBar;
		protected BarCompressor()
		{
			this.bar = null;
		}
		public static BarCompressor GetCompressor(global::OpenQuant.API.BarType barType, long oldBarSize, long newBarSize)
		{
			BarCompressor barCompressor;
			switch (barType)
			{
			case global::OpenQuant.API.BarType.Time:
				barCompressor = new TimeBarCompressor();
				break;
			case global::OpenQuant.API.BarType.Tick:
				barCompressor = new TickBarCompressor();
				break;
			case global::OpenQuant.API.BarType.Volume:
				barCompressor = new VolumeBarCompressor();
				break;
			case global::OpenQuant.API.BarType.Range:
				barCompressor = new RangeBarCompressor();
				break;
			default:
				throw new ArgumentException(string.Format("Unknown bar type - {0}", barType));
			}
			barCompressor.oldBarSize = oldBarSize;
			barCompressor.newBarSize = newBarSize;
			return barCompressor;
		}
		protected abstract void Add(DataEntry entry);
		protected void AddItemsToBar(PriceSizeItem[] items)
		{
			for (int i = 0; i < items.Length; i++)
			{
				PriceSizeItem item = items[i];
				this.AddItemToBar(item);
			}
		}
		protected void CreateNewBar(global::OpenQuant.API.BarType barType, DateTime beginTime, DateTime endTime, double price)
		{
			if (barType == global::OpenQuant.API.BarType.Time && this.newBarSize == 86400L)
			{
				this.bar = new global::OpenQuant.API.Bar(new Daily(beginTime, price, price, price, price, 0L));
				return;
			}
			this.bar = new global::OpenQuant.API.Bar(new SmartQuant.Data.Bar(EnumConverter.Convert(barType), this.newBarSize, beginTime, endTime, price, price, price, price, 0L, 0L));
		}
		protected void EmitNewCompressedBar()
		{
			if (this.NewCompressedBar != null)
			{
				this.NewCompressedBar(this, new CompressedBarEventArgs(this.bar));
			}
		}
		public BarSeries Compress(DataEntryEnumerator enumerator)
		{
			BarSeries series = new BarSeries();
			this.NewCompressedBar += delegate(object sender, CompressedBarEventArgs args)
			{
				series.Add(args.Bar);
			};
			while (enumerator.MoveNext())
			{
				this.Add(enumerator.Current);
			}
			this.Flush();
			return series;
		}
		private void AddItemToBar(PriceSizeItem item)
		{
			if (item.Price < this.bar.Low)
			{
				this.bar.bar.Low = item.Price;
			}
			if (item.Price > this.bar.High)
			{
				this.bar.bar.High = item.Price;
			}
			this.bar.bar.Close = item.Price;
			this.bar.bar.Volume += (long)item.Size;
		}
		private void Flush()
		{
			if (this.bar != null)
			{
				this.EmitNewCompressedBar();
			}
		}
	}
}
