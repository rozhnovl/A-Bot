using System.Reflection;
using Bib3.RefBaumKopii;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using Fasterflect;

namespace Bib3
{
	public class PropertyGenTimespanInt64<ValueT> : PropertyGenIntervalInt64<ValueT>, ITimespanInt64
	{
		public long Begin
		{
			set => this.Low = value;
			get => this.Low;
		}

		public long End
		{
			set => this.Up = value;
			get => this.Up;
		}

		public PropertyGenTimespanInt64()
		{
		}

		public PropertyGenTimespanInt64(ValueT value, long begin, long end)
			: base(value, begin, end)
		{
		}

		public PropertyGenTimespanInt64(ValueT value, long time)
			: this(value, time, time)
		{
		}

		public PropertyGenTimespanInt64(ValueT value, IIntervalInt64 interval)
			: base(value, interval)
		{
		}
	}
	public class PropertyGenIntervalInt64<ValueT> :
		IntervalInt64,
		IPropertyGenIntervalInt64<ValueT>,
		IIntervalInt64
	{
		public ValueT Value { set; get; }

		public PropertyGenIntervalInt64()
		{
		}

		public PropertyGenIntervalInt64(ValueT value, IIntervalInt64 @base = null)
			: base(@base)
		{
			this.Value = value;
		}

		public PropertyGenIntervalInt64(ValueT value, long low, long up)
			: base(low, up)
		{
			this.Value = value;
		}

		public PropertyGenIntervalInt64(ValueT value, long point)
			: base(point, point)
		{
			this.Value = value;
		}
	}
	public interface ITimespanInt64
	{
		long Begin { get; }

		long End { get; }
	}
	public class IntervalInt64 : IIntervalInt64
	{
		public long Low { set; get; }

		public long Up { set; get; }

		public IntervalInt64()
		{
		}

		public IntervalInt64(IIntervalInt64 @base)
		{
			this.Low = @base != null ? @base.Low : 0L;
			this.Up = @base != null ? @base.Up : 0L;
		}

		public IntervalInt64(long low, long up)
		{
			this.Low = low;
			this.Up = up;
		}
	}
	public interface IIntervalInt64
	{
		long Low { get; }

		long Up { get; }
	}
	public interface IPropertyGenIntervalInt64<ValueT> : IIntervalInt64
	{
		ValueT Value { get; }
	}
}
