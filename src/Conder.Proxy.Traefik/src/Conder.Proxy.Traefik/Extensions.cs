namespace Conder.Proxy.Traefik
{
    public static class Extensions
    {
        private const string SectionName = "traefik";
        private const string RegistryName = "proxy.traefik";

        public static IConderBuilder AddTraefik(this IConderBuilder builder, string sectionName = SectionName,
            string consulSectionName = "consul", string httpClientSectionName = "httpClient")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = sectionName;
            }
        }
    }
}