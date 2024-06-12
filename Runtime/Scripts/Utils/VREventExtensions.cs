namespace IVLab.MinVR3
{
    public static class VREventExtensions
    {
        public static string GetJsonData(this VREvent vrEvent)
        {
            return vrEvent.GetData<string>().Replace("\\\"", "\"");
        }
    }
}