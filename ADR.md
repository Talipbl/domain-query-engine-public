# Mimari Karar Kayıtları (ADR)

Bu dosya, projede verilen önemli mimari kararları, gerekçelerini ve sonuçlarını kaydeder. Yeni bir karar alındığında sona yeni bir başlık eklenir; eski bir karar değişirse silinmez, durumu "Yerini ADR-XXX aldı" olarak güncellenir.

Biçim: **Bağlam** (sorun neydi), **Karar** (ne yapıldı), **Sonuçlar** (kazanılan ve bedeli).

| No | Başlık | Durum |
|---|---|---|
| 001 | Katmanlı çözüm yapısı | Kabul edildi |
| 002 | Sağlayıcı başına strateji + parser | Kabul edildi |
| 003 | Bot korumaları aşılmaz | Kabul edildi |
| 004 | Paralel sorgu, sağlayıcı başına zaman aşımı ve önbellek | Kabul edildi |
| 005 | Karşılaştırmada kampanya fiyatı | Kabul edildi |
| 006 | TCMB kuruyla para birimi çevirme | Kabul edildi |
| 007 | Sunucu tarafı Razor + htmx arayüz | Kabul edildi |
| 008 | Sade CSS tasarım sistemi ve token'lar | Kabul edildi |
| 009 | Katalog verisi arayüzlerin arkasında, önce Mock | Kabul edildi |
| 010 | Sorgu geçmişi ve istatistikler | Kabul edildi |
| 011 | Çerez tabanlı tercihler | Kabul edildi |
| 012 | JSON anahtar/değer yerelleştirme (TR/EN) | Kabul edildi |
| 013 | Açık/koyu tema | Kabul edildi |
| 014 | Gizli bilgiler depoda tutulmaz | Kabul edildi |
| 015 | Docker Compose ile uygulama + PostgreSQL | Kabul edildi |

---

## ADR-001: Katmanlı çözüm yapısı

**Bağlam:** Sorgu mantığı, sağlayıcı entegrasyonları, veri erişimi ve arayüz birbirinden bağımsız değişiyor. Sağlayıcı siteleri sık değişir; arayüz ve veritabanı bundan etkilenmemeli.

**Karar:** Çözüm beş projeye ayrıldı: `Core` (modeller ve soyutlamalar), `Application` (servisler), `Infrastructure` (sağlayıcılar, HTTP, kur, Mock veri), `PostgreSQL` (EF Core) ve `SearchUI` (web). Bağımlılıklar Core'a doğrudur; Core hiçbir projeye bağımlı değildir. Kayıtlar her projenin `ServiceRegistrations` sınıfında yapılır.

**Sonuçlar:** Sağlayıcı veya veri kaynağı değişikliği yalnızca kendi projesine dokunur, testlerde sahte uygulamalarla değiştirilebilir. Bedeli: küçük bir proje için fazladan proje ve arayüz sayısı.

## ADR-002: Sağlayıcı başına strateji + parser

**Bağlam:** Her sağlayıcı farklı sorgulanır (WHMCS + CSRF, JSON API, JSONP, HTML form) ve farklı yanıt döner.

**Karar:** Her sağlayıcı bir `IDomainLookupStrategy` (isteği yapar) ve bir parser (yanıtı ortak `DomainResult` modeline çevirir) olarak yazılır. Parser'lar gerçek sitelerden alınmış kayıtlı yanıtlarla birim testine tabidir. Canlı sitelere giden testler `Category=Integration` ile ayrılır.

**Sonuçlar:** Yeni sağlayıcı eklemek mevcut kodu değiştirmez. Bir site değiştiğinde yalnızca o sağlayıcının parser'ı bozulur ve testlerde görülür. Bedeli: sağlayıcılar sitelerini değiştirdikçe bakım gerekir.

## ADR-003: Bot korumaları aşılmaz

**Bağlam:** Bazı sağlayıcılar sunucu taraflı isteklere Cloudflare kontrolü veya resimli doğrulama (captcha) ile yanıt verir.

**Karar:** Bot korumaları, captcha'lar ve erişim kısıtlamaları aşılmaya çalışılmaz. Böyle yanıt veren sorgu "başarısız" raporlanır (örnek: Turkticaret `E_KEY_ERROR` → "Sağlayıcı doğrulama istedi"). Sürekli engelleyen sağlayıcı kayıttan çıkarılır (örnek: Güzel Hosting).

**Sonuçlar:** Sağlayıcıların kurallarına uyulur, hukuki ve etik risk alınmaz. Bedeli: bazı sağlayıcılar karşılaştırmada yer almaz.

## ADR-004: Paralel sorgu, sağlayıcı başına zaman aşımı ve önbellek

**Bağlam:** Sağlayıcılar tek tek sorulursa sayfa çok yavaş açılır; yavaş bir sağlayıcı tüm sonucu bekletir. Aynı domain için tekrar tekrar sorgu atmak sağlayıcılara gereksiz yük bindirir.

**Karar:** Tüm sağlayıcılar paralel sorgulanır. Her sağlayıcıya 8 saniye verilir; aşan sağlayıcı "zamanında yanıt vermedi" olarak gösterilir. Sonuçlar domain başına bellekte 5 dakika (bir sağlayıcı başarısızsa 1 dakika) tutulur. Filtre, sıralama ve para birimi değişiklikleri önbellekten yeniden gösterilir ve yeni arama sayılmaz. HTTP isteklerinde Polly ile 2 yeniden deneme yapılır.

**Sonuçlar:** Sayfa en yavaş sağlayıcıyı beklemez; sağlayıcılara giden istek sayısı sınırlı kalır. Bedeli: fiyatlar 5 dakikaya kadar eski olabilir.

## ADR-005: Karşılaştırmada kampanya fiyatı

**Bağlam:** Sağlayıcıların çoğu ilk alım için indirimli fiyat sunar. Kullanıcı ilk alımda ödeyeceği fiyatı görmek ister.

**Karar:** Sağlayıcı kampanya fiyatı veriyorsa sıralama ve "en ucuz" hesabı kampanya fiyatıyla, yoksa normal fiyatla yapılır. Kampanyalı fiyatın yanında normal fiyat üstü çizili gösterilir ve kampanyaların genellikle yalnızca ilk alımda geçerli olduğu belirtilir. Diğer sağlayıcılardan aşırı yüksek fiyatlar uyarıyla işaretlenir.

**Sonuçlar:** Liste kullanıcının gerçekte ödeyeceği ilk fiyatı yansıtır. Bedeli: yenileme maliyeti ilk bakışta görünmez; uzantı sayfalarında yenileme fiyatı ayrıca gösterilir.

## ADR-006: TCMB kuruyla para birimi çevirme

**Bağlam:** Bazı sağlayıcılar fiyatı USD veya EUR ile verir. Farklı para birimleri doğrudan karşılaştırılamaz.

**Karar:** Fiyatlar TCMB'nin günlük döviz satış kuruyla (saatte bir yenilenir) kullanıcının seçtiği para birimine (TRY, USD, EUR) çevrilir. Çevrilen fiyatlar "≈" ile işaretlenir ve sağlayıcının orijinal fiyatı da gösterilir. Kur alınamazsa fiyatlar sağlayıcının kendi para biriminde, TRY önce gösterilir. Geçmişte orijinal fiyatlar saklanır.

**Sonuçlar:** Tüm sağlayıcılar tek para biriminde sıralanır ve kaynak resmi bir kurdur. Bedeli: sağlayıcının kendi kur farkı veya vergileri yansımayabilir; bu yüzden fiyatlar "bilgi amaçlı" olarak sunulur.

## ADR-007: Sunucu tarafı Razor + htmx arayüz

**Bağlam:** Arayüzün hızlı açılması, arama motorlarında görünmesi (uzantı ve sağlayıcı sayfaları), JavaScript olmadan da çalışması ve tek bir .NET projesinde kalması isteniyor.

**Karar:** Sayfalar ASP.NET Core MVC Razor view'larıyla sunucuda çizilir. Etkileşim (arama sonuçlarının yüklenmesi, filtreler, sekmeler, sıralama) htmx ile yapılır; htmx `wwwroot/lib/htmx` altında yerel kopya olarak sunulur. SPA framework'ü ve frontend build adımı kullanılmaz. Arama sayfası hemen açılır, sonuçlar `/ara/sonuclar`'dan ayrı yüklenir. Adresler Türkçedir; katalog sayfalarında canonical/Open Graph etiketleri, arama sayfalarında `noindex`, ayrıca `/sitemap.xml` ve `/robots.txt` vardır.

**Sonuçlar:** Tek dil (C#) ve tek dağıtım; SEO ve erişilebilirlik kolay. Bedeli: zengin istemci etkileşimleri sınırlıdır; karmaşık arayüz parçaları için ileride yeniden değerlendirilebilir.

## ADR-008: Sade CSS tasarım sistemi ve token'lar

**Bağlam:** Tutarlı bir görünüm gerekiyor ama build adımı olmadan.

**Karar:** Framework kullanılmaz. Renk, yazı, boşluk ve gölgeler `tokens.css` içinde CSS değişkenleri olarak tanımlanır; `base.css` temel stiller, `components.css` bileşen sınıfları içerir. Tekrarlanan arayüz parçaları tag helper (`<badge>`, `<btn>`, `<price-tag>`) veya partial'dır. Geliştirme ortamında `/_bilesenler` galerisi bileşenleri gösterir.

**Sonuçlar:** Bağımlılık yok, tema değişikliği yalnızca token'lardan yapılır. Bedeli: hazır bileşen kütüphanesi yok; her bileşen elle yazılır.

## ADR-009: Katalog verisi arayüzlerin arkasında, önce Mock

**Bağlam:** Uzantı listesi, sağlayıcı bilgileri ve fiyat geçmişi için henüz gerçek veri kaynağı yok, ama sayfaların şimdiden tasarlanması gerekiyor.

**Karar:** Katalog ve istatistik verileri `ITldCatalog`, `IProviderCatalog` ve `ISearchStatistics` arayüzlerinin arkasındadır. İlk uygulamalar `Infrastructure/MockData` altındaki gömülü JSON'u okur. Kaynak `DataSource:Tlds`, `DataSource:Providers` ve `DataSource:Statistics` ayarlarıyla seçilir. Örnek veri sayfalarda "Örnek veri" rozetiyle işaretlenir. Domain sorgusunun kendisi her zaman canlıdır.

**Sonuçlar:** Gerçek veri kaynağı geldiğinde yalnızca yeni bir arayüz uygulaması yazılır; view'lar değişmez. Bedeli: Mock verideki fiyatlar gerçeği yansıtmaz, bu yüzden açıkça işaretlenir.

## ADR-010: Sorgu geçmişi ve istatistikler

**Bağlam:** Hangi domainlerin ve uzantıların ne sıklıkla sorgulandığını görmek isteniyor.

**Karar:** Her arama `Domains` tablosuna (domain, uzantı, oturum, IP, tarayıcı) ve sağlayıcı sonuçları `DomainLookupResults` tablosuna yazılır. Ana sayfadaki "Son 24 saatte en çok sorgulanan uzantılar" listesi `Domains` tablosu uzantıya göre gruplanarak üretilir; tek tek domainler gösterilmez. Önbellekten yeniden gösterim yeni arama sayılmaz.

**Sonuçlar:** İstatistik doğrudan kendi verimizden gelir. Bedeli: kişisel veri (IP, tarayıcı) saklanır; gizlilik sayfasında belirtilir ve bu veriler dış servislere gönderilmez. KVKK metni yayından önce tamamlanmalıdır.

## ADR-011: Çerez tabanlı tercihler

**Bağlam:** Para birimi, tema ve dil seçimi sayfalar arasında korunmalı, ilk çizimde doğru görünmeli ve JavaScript olmadan da çalışmalı. Kullanıcı hesabı yok.

**Karar:** Tercihler çerezlerde tutulur: `dqe_currency`, `dqe_theme`, ASP.NET'in `.AspNetCore.Culture` çerezi. Her biri `/tercihler/para-birimi`, `/tercihler/tema`, `/tercihler/dil` adreslerine antiforgery korumalı POST ile değişir. JavaScript varsa istek `fetch` ile atılır ve sayfa yenilenmeden uygulanır (204), yoksa form gönderilir ve aynı sayfaya geri dönülür. Tercih çerezleri "gerekli" olarak işaretlenir (takip amaçlı değildir).

**Sonuçlar:** Sunucu sayfayı ilk seferde doğru tercihle çizer; hesap gerekmez. Bedeli: tercihler cihaz/tarayıcı başınadır.

## ADR-012: JSON anahtar/değer yerelleştirme (TR/EN)

**Bağlam:** Arayüz Türkçe ve İngilizce olmalı; metinlerin kolayca düzenlenmesi ve yeni dil eklenebilmesi isteniyor. `.resx` dosyaları elle düzenlemeye uygun değil.

**Karar:** Tüm metinler `SearchUI/Resources/Locales/{dil}.json` dosyalarında düz anahtar/değer olarak tutulur ve derlemeye gömülür. Özel bir `IStringLocalizerFactory` (`JsonStringLocalizer`) bunları okur; view'lar standart `IStringLocalizer<SharedResource>` ile kullanır. Varsayılan dil `tr-TR`; dil sırası: sorgu dizesi → çerez → tarayıcı dili. Eksik İngilizce anahtar Türkçeye düşer ve loglanır. Sayı 1 olduğunda `Anahtar.One` biçimi kullanılır. Fiyat, sayı ve tarih biçimi kültürü izler. Katalogdaki açıklama ve SSS metinleri için ayrı İngilizce alanlar vardır. Adresler (`/uzantilar` vb.) dile göre değişmez. Testler iki dosyadaki anahtarların aynı olduğunu ve koddaki her anahtarın var olduğunu denetler.

**Sonuçlar:** Metinler tek yerde, kolay düzenlenir; yeni dil = yeni JSON + `LanguagePreference.Languages` kaydı. Bedeli: karmaşık çoğul kuralları yok (yalnızca tekil/çoğul); İngilizce sayfaların ayrı adresi olmadığı için arama motorlarında ayrı dizinlenmez.

## ADR-013: Açık/koyu tema

**Bağlam:** Kullanıcı temayı seçebilmeli; seçim yoksa sistem ayarı izlenmeli ve sayfa açılırken yanlış temada yanıp sönmemeli.

**Karar:** Koyu tema token'ları `:root[data-theme="dark"]` ve `prefers-color-scheme: dark` medya sorgusuyla tanımlanır. Sunucu seçimi çerezden okuyup `<html data-theme>`'yi doldurur; seçim yoksa değer boştur ve sistem ayarı geçerlidir. Başlıktaki düğme temayı yenilemeden değiştirir ve seçimi kaydeder. Sağlayıcı logoları her iki temada okunabilsin diye satır başında beyaz arka planlı bir kutuda gösterilir.

**Sonuçlar:** Yanıp sönme olmaz, JS olmadan da çalışır. Bedeli: yeni renk eklenirken koyu tema karşılığı da tanımlanmalıdır.

## ADR-014: Gizli bilgiler depoda tutulmaz

**Bağlam:** Depo herkese açıktır; veritabanı bağlantı dizesi ve parolalar sızmamalı.

**Karar:** `appsettings.json` içindeki bağlantı dizesi boştur ve uygulama bağlantı dizesi olmadan başlamaz. Yerelde User Secrets, konteynerde ortam değişkenleri (`ConnectionStrings__DefaultConnection`) veya gitignore'daki `.env` dosyası kullanılır. Gizli değerler loglara veya çıktılara yazılmaz.

**Sonuçlar:** Depoda gizli bilgi bulunmaz. Bedeli: her ortamın kurulumu ayrıca yapılmalıdır (README'de anlatılır).

## ADR-015: Docker Compose ile uygulama + PostgreSQL

**Bağlam:** Uygulamanın tek komutla, veritabanıyla birlikte ayağa kalkması isteniyor.

**Karar:** `Dockerfile` web uygulamasını Debian tabanlı `aspnet` imajında çalıştırır (dil ve sayı biçimleri için ICU gerekir; Alpine kullanılmaz). `docker-compose.yml` uygulamayı ve `postgres:16`'yı birlikte çalıştırır; veritabanı sağlık kontrolünden geçmeden uygulama başlamaz, veriler kalıcı bir volume'dadır, parola `.env`'den gelir. Migration'lar uygulama açılışında uygulanır (`Database:MigrateOnStartup`). Geliştirmede yalnızca PostgreSQL konteyneri kullanılıp uygulama `dotnet run` ile de çalıştırılabilir. Docker kurulumu ve konteyner yönetimi geliştiricinin kendisindedir.

**Sonuçlar:** Kurulum basit ve tekrarlanabilir. Bedeli: açılışta migration uygulamak çok örnekli dağıtımlarda yarışa yol açabilir; o durumda ayrı bir migration adımına geçilmelidir.
