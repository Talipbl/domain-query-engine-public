namespace DomainQueryEngine.Infrastructure.Test.Fixtures
{
    /// <summary>
    /// Provider responses used by the parser unit tests.
    /// <para>
    /// <see cref="WhmcsAvailable"/> is a real response that was
    /// captured earlier (it comes from the original tests). The others are minimal samples written
    /// to match the structure each parser reads; replace them with real captures when convenient.
    /// </para>
    /// </summary>
    public static class SampleResponses
    {
        /// <summary>WHMCS "domain/check" response (Yildiz Bilisim, GriNet, Alan Adi Kayit, IsimKayit).</summary>
        public const string WhmcsAvailable = """
            {"result":[{"domainName":"salihotoyikama.com","idnDomainName":"salihotoyikama.com","tld":"com","tldNoDots":"com","sld":"salihotoyikama","idnSld":"salihotoyikama","status":"available for registration","legacyStatus":"available","score":1,"isRegistered":false,"isAvailable":true,"isValidDomain":true,"domainErrorMessage":"","pricing":{"1":{"register":"₺4000.00 TL","transfer":"₺4000.00 TL","renew":"₺4000.00 TL"},"5":{"register":"₺18000.00 TL","renew":"₺18000.00 TL"},"10":{"register":"₺27000.00 TL"}},"shortestPeriod":{"period":1,"register":"₺4000.00 TL","transfer":"₺4000.00 TL","renew":"₺4000.00 TL"},"group":"hot","minLength":3,"maxLength":63,"isPremium":false,"premiumCostPricing":[]}]}
            """;

        /// <summary>WHMCS response for a registered domain: no pricing block.</summary>
        public const string WhmcsTaken = """
            {"result":[{"domainName":"google.com","tld":"com","isRegistered":true,"isAvailable":false,"isValidDomain":true,"pricing":null,"isPremium":false}]}
            """;

        /// <summary>WHMCS response whose pricing lacks the shortest period (IsimKayit adds it).</summary>
        public const string WhmcsShortestPeriodOnly = """
            {"result":[{"domainName":"ornek.com.tr","tld":"com.tr","isAvailable":true,"isValidDomain":true,"pricing":{"2":{"register":"₺900.00 TL"}},"shortestPeriod":{"period":1,"register":"₺500.00 TL","renew":"₺550.00 TL"},"isPremium":false}]}
            """;

        public const string NatroAvailable = """
            {"serviceResponse":{"api":{"response":{"result":{"code":1000},"msg":"success","resdata":{"domain":{"avail":"1","ext":"com","price":"9.99","minyear":1,"text":"ornek.com"}}}}}}
            """;

        public const string NatroTaken = """
            {"serviceResponse":{"api":{"response":{"result":{"code":1000},"msg":"success","resdata":{"domain":{"avail":"0","ext":"com","price":"9.99","minyear":1,"text":"google.com"}}}}}}
            """;

        public const string IhsAvailable = """
            <div class="domain__box__query__results">
              <div id="musait-ornek-com" class="domain__box__query__results--item available">
                <span class="domain__box__query__results--item-domain-name">ornek.com</span>
                <span class="show-us-curr">$12.99</span>
                <span class="show-tl-curr">449,90 TL</span>
              </div>
              <div id="dolu-google-com" class="domain__box__query__results--item">
                <span class="domain__box__query__results--item-domain-name">google.com</span>
              </div>
            </div>
            """;

        /// <summary>IHS row with a price block but no readable TL price.</summary>
        public const string IhsWithoutTlPrice = """
            <div id="musait-ornek-net" class="domain__box__query__results--item available">
              <span class="domain__box__query__results--item-domain-name">ornek.net</span>
              <span class="show-us-curr">$14.99</span>
            </div>
            """;

        public const string IsimTescilAvailable = """
            <table>
              <tr>
                <td><b data-domainname="ornek.com" data-section="1">ornek.com</b></td>
                <td><span class="text-decoration-line-through">₺599,00</span><span class="price">₺399,00</span></td>
                <td><a data-section="1">Müsait - Kayıt Et</a></td>
              </tr>
            </table>
            """;

    }
}
