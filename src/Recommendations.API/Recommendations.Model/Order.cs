namespace Recommendations.Model
{
    public sealed class Order
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int Day { get; set; }


        bool Equals(Order other)
        {
            return ID == other.ID && UserID == other.UserID && Day == other.Day;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Order other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ID;
                hashCode = (hashCode * 397) ^ UserID;
                hashCode = (hashCode * 397) ^ Day;
                return hashCode;
            }
        }
    }
}
