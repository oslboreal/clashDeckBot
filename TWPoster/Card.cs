namespace TWPoster
{
    public class Card
    {
        public Card(int arena, string description, int elixir, string icon, int id, string key, int minLevel, int maxLevel, int starLevel, string name, string rarity, string type)
        {
            Arena = arena;
            Description = description;
            Elixir = elixir;
            Icon = icon;
            Id = id;
            Key = key;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            StarLevel = starLevel;
            Name = name;
            Rarity = rarity;
            Type = type;
        }

        public int Arena { get; set; }
        public string Description { get; set; }
        public int Elixir { get; set; }
        public string Icon { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public int StarLevel { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Type { get; set; }


    }
}
