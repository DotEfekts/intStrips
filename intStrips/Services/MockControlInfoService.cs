namespace intStrips.Services
{
    public class MockControlInfoService : IControlInfoService
    {
        public string GetControllingAerodrome() => "YMML";
    }
}