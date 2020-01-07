namespace com.caucho.test
{
    public class Car
    {
        public string model;
        public string color;
        public int mileage;

        protected bool Equals(Car other)
        {
            return string.Equals(model, other.model) && string.Equals(color, other.color) && mileage == other.mileage;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Car)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (model != null ? model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ mileage;
                return hashCode;
            }
        }
    }
}