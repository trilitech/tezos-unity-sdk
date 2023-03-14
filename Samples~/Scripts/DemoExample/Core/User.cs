public class User : IUserModel
{
    public User(string name, string id, string address)
    {
        Name = name;
        Identifier = id;
        Address = address;
    }
    
    public string Name { get; }
    public string Identifier { get; }
    public string Address { get; private set; }

    public void UpdateAddress(string address)
    {
        Address = address;
    }
}
