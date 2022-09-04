using Newtonsoft.Json;

var apiKey = System.Environment.GetEnvironmentVariable("API_KEY");

var weathers = GetWeathers().ToList();

var orderedByTemps = (from w in weathers
                                orderby w.Temp descending 
                                select w).ToList();
Console.WriteLine($"Max Temp: {orderedByTemps.First().Country}.\nMin Temp: {orderedByTemps.Last().Country}.");

var averageTemp = (from w in weathers select w.Temp).Average();
Console.WriteLine($"Average Temp: {averageTemp}.");

var countries = (from w in weathers select w.Country).Distinct().Count();
Console.WriteLine($"# of Countries: {countries}.");

var firstClear = (from w in weathers where w.Description == "clear sky" select w).FirstOrDefault();
var firstRain = (from w in weathers where w.Description == "rain" select w).FirstOrDefault();
var firstClouds = (from w in weathers where w.Description == "few clouds" select w).FirstOrDefault();

Console.WriteLine(firstClear == null ? "No \"clear sky\"" : $"First \"clear sky\": {firstClear.Name}.");
Console.WriteLine(firstRain == null? "No \"rain\"" : $"First \"rain\": {firstRain.Name}.");
Console.WriteLine(firstClouds == null ? "No \"few clouds\"" : $"First \"few clouds\": {firstClouds.Name}.");

IEnumerable<Weather> GetWeathers()
{
    var client = new HttpClient();
    var count = 0;
    Random random = new();

    for (; count < 50;)
    {
        var lat = random.NextDouble() * (180) - 90;
        var lon = random.NextDouble() * (360) - 180;

        Weather? weather;
        
        try
        {
            var resp = client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={apiKey}")
                .Result.Content.ReadAsStringAsync().Result;

            dynamic? json = JsonConvert.DeserializeObject(resp);
            if (json == null) continue;
            
            weather = new Weather((string)json.name, (string)json.sys.country, (double)json.main.temp,
                (string)json.weather[0].description);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            weather = null;
        }

        if (weather == null) continue;
        
        if (string.IsNullOrEmpty(weather.Name) || string.IsNullOrEmpty(weather.Country) ||
            string.IsNullOrEmpty(weather.Description)) continue;
        
        count++;
        yield return weather;
    }
}

class Weather
{
    public string Name { get; private set; }
    public string Country { get; private set; }
    public double Temp { get; private set; }
    public string Description { get; private set; }
    
    public Weather(string name, string country, double temp, string description)
    {
        Name = name;
        Country = country;
        Temp = temp;
        Description = description;
    }
}

