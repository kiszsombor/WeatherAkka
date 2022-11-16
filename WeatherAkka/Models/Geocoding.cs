namespace WeatherAkka.Models
{
    public class Geocoding
    {
        private double latitude;
        private double longitude;

        public double Latitude { get => latitude; set => latitude = value; }
        public double Longitude { get => longitude; set => longitude = value; }

        public override string ToString()
        {
            return Latitude + " " + Longitude;
        }
    }
}
