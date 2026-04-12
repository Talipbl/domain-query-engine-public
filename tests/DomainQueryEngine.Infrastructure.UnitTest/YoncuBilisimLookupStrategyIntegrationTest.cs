using DomainQueryEngine.YoncuBilisim;
using Moq;

namespace DomainQueryEngine.Infrastructure.Test
{
    public class YoncuBilisimLookupStrategyIntegrationTest
    {
        YoncuBilisimLookupStrategy _lookupStrategy;
        YoncuBilisimStrategyParser _parser;
        public YoncuBilisimLookupStrategyIntegrationTest()
        {
            _parser = new YoncuBilisimStrategyParser();
            _lookupStrategy = new YoncuBilisimLookupStrategy(_parser); 
        }

        [Fact]
        public async Task Should_Lookup_Domain_And_Parse_Result()
        {
            var result = await _lookupStrategy.LookupAsync("salihotoyikama.com");

            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Parse_Domain_Result()
        {
            string lookupResponse = "<code id=\"ekle_salihotoyikama2.com\">salihotoyikama2.com</code><strong>Boş</strong><a class=\"full\"><h5><select id=\"hssalihotoyikama2.com\">\t\t\t\t\t<option value=\"1\">&nbsp;&nbsp;1 Yil&nbsp;&nbsp;/&nbsp;&nbsp;580&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"2\">&nbsp;&nbsp;2 Yil&nbsp;&nbsp;/&nbsp;&nbsp;1260&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"3\">&nbsp;&nbsp;3 Yil&nbsp;&nbsp;/&nbsp;&nbsp;1940&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"4\">&nbsp;&nbsp;4 Yil&nbsp;&nbsp;/&nbsp;&nbsp;2620&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"5\">&nbsp;&nbsp;5 Yil&nbsp;&nbsp;/&nbsp;&nbsp;3300&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"6\">&nbsp;&nbsp;6 Yil&nbsp;&nbsp;/&nbsp;&nbsp;3980&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"7\">&nbsp;&nbsp;7 Yil&nbsp;&nbsp;/&nbsp;&nbsp;4660&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"8\">&nbsp;&nbsp;8 Yil&nbsp;&nbsp;/&nbsp;&nbsp;5340&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"9\">&nbsp;&nbsp;9 Yil&nbsp;&nbsp;/&nbsp;&nbsp;6020&nbsp;₺</option>\r\n\t\t\t\t\t<option value=\"10\">10 Yil&nbsp;&nbsp;/&nbsp;&nbsp;6700&nbsp;₺</option>\r\n</select><input type=\"hidden\" value=\"1\"/></h5><h4 style=\"cursor:pointer;\" onclick=\"sepetime_ekle('ekle_salihotoyikama2.com','12',encodeURIComponent('alan_adi=salihotoyikama2.com&alan_adi_durumu=1&uzanti_id=49&hizmet_suresi='+document.getElementById('hssalihotoyikama2.com').value));\">Sepete Ekle</h4></a>";

            var parseResult = _parser.ParseDomainResult(lookupResponse);
        }
    }
}