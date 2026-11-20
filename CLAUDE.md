# CLAUDE.md

Bu dosya, bu depoda çalışan Claude (ve diğer geliştiriciler) için proje özeti, çalışma kuralları ve sınırlardır. Mimari kararların gerekçeleri [ADR.md](ADR.md) dosyasındadır; kurulum ve kullanım için [README.md](README.md).

## Proje

**En Ucuz Domain**: Bir alan adının müsaitlik durumunu ve kayıt fiyatını Türk domain sağlayıcılarına aynı anda sorup tek listede karşılaştıran .NET 8 web uygulaması. Kampanya fiyatı varsa kampanya fiyatı, yoksa normal fiyat karşılaştırılır. Döviz fiyatları TCMB kuruyla seçilen para birimine (TRY/USD/EUR) çevrilir.

### Çözüm yapısı

| Proje | Sorumluluk |
|---|---|
| `src/DomainQueryEngine.Core` | Varlıklar, modeller, soyutlamalar (`IDomainLookupStrategy`, `ITldCatalog`, `IProviderCatalog`, `ISearchStatistics`, `IExchangeRateProvider`), yardımcılar. Başka projeye bağımlı değildir. |
| `src/DomainQueryEngine.Application` | Sorgu orkestrasyonu (`LookupService`, önbellekli sürümü), fiyat karşılaştırma ve kur çevirme (`PriceComparisonService`), uzantı fiyat sayfaları (`TldPriceService`), arama kaydı. |
| `src/DomainQueryEngine.Infrastructure` | Sağlayıcı başına strateji + parser (`Strategies/`, `Parsers/`, `Catalog/`), HTTP istemcisi ve TCMB kur sağlayıcısı. |
| `src/DomainQueryEngine.PostgreSQL` | EF Core `DbContext`, migration'lar, repository'ler, veritabanı istatistikleri. |
| `src/DomainQueryEngine.SearchUI` | ASP.NET Core MVC (Razor + htmx) web uygulaması ve `/api/lookup` API'si. |
| `tests/*` | Birim testleri ve canlı sitelere giden entegrasyon testleri. |

Bağımlılık yönü: `SearchUI → Application → Core ← Infrastructure / PostgreSQL`. Core hiçbir dış projeye referans vermez.

### Teknoloji

.NET 8, ASP.NET Core MVC (Razor view, tag helper), htmx 2.0.4 (`wwwroot/lib/htmx`, yerel kopya), sade CSS (framework yok, frontend build adımı yok), EF Core + Npgsql (PostgreSQL 16), Polly, Newtonsoft.Json, xUnit.

### UI özeti

- Sayfalar sunucuda çizilir; htmx arama sonuçlarını (`/ara/sonuclar`), filtreleri, sekmeleri ve sıralamayı tam sayfa yüklemeden günceller. Her şey JavaScript olmadan da link/form olarak çalışır.
- Adresler Türkçedir: `/ara`, `/uzantilar`, `/uzantilar/{slug}` (`com-tr`), `/saglayicilar`, `/gizlilik`, `/tercihler/*`. Geliştirme ortamında bileşen galerisi: `/_bilesenler`.
- Tasarım sistemi: `wwwroot/css/tokens.css` (açık tema `:root`, koyu tema `[data-theme="dark"]` ve sistem ayarı), `base.css`, `components.css`.
- Bileşenler: `<badge>`, `<btn>`, `<price-tag>` tag helper'ları ve `Views/Shared/Components/*` partial'ları.
- Tercihler çerezde: `dqe_currency`, `dqe_theme`, `.AspNetCore.Culture`. Her biri `/tercihler/*` altında antiforgery korumalı POST ile değişir; JS varsa fetch ile, yoksa form ile.
- Diller: Türkçe (varsayılan) ve İngilizce. Tüm metinler `Resources/Locales/tr.json` ve `en.json` içinde anahtar/değer olarak durur.

## Komutlar

```powershell
# derle
dotnet build DomainQueryEngine.sln

# birim testleri (varsayılan; ağ ve veritabanı gerekmez)
dotnet test DomainQueryEngine.sln --filter "Category!=Integration"

# uygulamayı çalıştır (User Secrets'ta bağlantı dizesi gerekir)
cd .\src\DomainQueryEngine.SearchUI; dotnet run
```

Önizleme sunucusu çalışırken derleme DLL kilidi yüzünden başarısız olur: önce sunucuyu durdur, sonra derle.

## Çalışma kuralları

### Kod

- Yeni sağlayıcı = `Infrastructure/Strategies/` altında bir `IDomainLookupStrategy` + `Parsers/` altında bir parser + `InfrastructureServiceRegistration`'da kayıt + gerçek yanıtla parser testi (`tests/.../Fixtures/Responses/`). Sağlayıcı başına ayrı commit tercih edilir.
- Kullanıcıya görünen her metin bir yerelleştirme anahtarıdır. View'larda `@T["Anahtar"]`, argümanlıysa `@T["Anahtar", değer]`. Yeni anahtar **hem `tr.json` hem `en.json`'a** eklenir. Sayı 1 olduğunda farklı biçim gerekiyorsa `Anahtar.One` eklenir. `LocalizationTests` iki dosyanın aynı anahtarlara sahip olduğunu ve koddaki her anahtarın var olduğunu denetler.
- Fiyat, sayı ve tarih biçimi için `PriceFormatter` kullanılır (kültüre göre biçimler); elle `ToString("N2", tr-TR)` yazılmaz.
- Renkler ve boşluklar `tokens.css` token'larıyla verilir; yeni renk eklenirse koyu tema karşılığı da eklenir.
- Sayfalar JavaScript olmadan da çalışmalıdır; JS yalnızca iyileştirmedir.
- Uzantı fiyatları ve arama istatistikleri PostgreSQL'den okunur; sağlayıcı profilleri kayıtlı fiyat listesi stratejilerinden üretilir. Fiyat listeleri zamanlanmış arka plan servisiyle yenilenir.
- Değişiklikten sonra birim testleri çalıştırılır; UI değişikliği tarayıcıda (açık/koyu tema, TR/EN, mobil genişlik) kontrol edilir.

### Git

- **Commit ve push yalnızca kullanıcı istediğinde.** Çalışma dalı `development`, ana dal `master`.
- Commit mesajları İngilizce, emir kipinde, kısa başlık + gerekirse madde listesi.
- `.claude/` klasörü (özellikle `launch.json`) commitlenmez.
- `.env` gitignore'dadır ve asla commitlenmez.

## Sınırlar

- **Gizli bilgiler:** Bağlantı dizesi, parola, token gibi değerler depoya yazılmaz, çıktıya veya loglara basılmaz. Yerelde User Secrets, konteynerde ortam değişkeni / `.env` kullanılır. `appsettings.json` içindeki bağlantı dizesi boş kalır.
- **Bot koruması:** Sağlayıcıların bot korumaları, captcha'ları veya Cloudflare kontrolleri aşılmaya çalışılmaz. Böyle yanıt veren sağlayıcı "başarısız" raporlanır ya da kayıttan çıkarılır (örnek: Güzel Hosting, Turkticaret `E_KEY_ERROR`).
- **Sağlayıcı siteleri:** Sağlayıcılara gereksiz istek gönderilmez. Sonuçlar 5 dakika önbellekte tutulur; filtre veya para birimi değişimi yeni sorgu sayılmaz. Entegrasyon testleri (`Category=Integration`) canlı sitelere gider, varsayılan olarak çalıştırılmaz.
- **Docker:** Docker kurulumu ve konteyner yönetimi kullanıcıdadır. İstenmedikçe imaj derlenmez, konteyner başlatılmaz veya durdurulmaz.
- **İndirmeler:** Yeni paket, kütüphane veya dosya indirmek için önce onay alınır. Frontend kütüphaneleri CDN'den değil `wwwroot/lib` altından sunulur.
- **Veritabanı:** Migration'lar uygulama açılışında uygulanır. Mevcut verileri silen veya şemayı geri alınamaz biçimde değiştiren işlemler onaysız yapılmaz.
- **Kişisel veri:** Arama kaydı IP ve tarayıcı bilgisini içerir; bu veriler dış servislere gönderilmez. Gizlilik sayfası güncel tutulur.
- **İletişim:** Kullanıcıyla Türkçe yazışılır.
