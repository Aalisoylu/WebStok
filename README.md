# WebStok

ASP.NET Core MVC ve SQLite ile geliştirilmiş, Türkçe stok ve depo yönetim uygulaması.

Proje Youtube linki
https://youtu.be/5Re71zD_h4g [![Tanıtım Videosu](https://img.shields.io/badge/YouTube-Tanıtım_Videosu-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://youtu.be/5Re71zD_h4g)


## Özellikler

- Genel bakış: kritik stoklar, yaklaşan son kullanma tarihleri ve son hareketler.
- Ürün, kategori ve depo tanımları.
- Mal kabul, stok çıkışı, depolar arası transfer ve iade işlemleri.
- FIFO / FEFO, lot takibi ve depo bazlı erişim.
- Mobil uyumlu yönetim paneli ve giriş ekranı.

## Yerel kurulum

.NET 10 SDK gereklidir. Proje kökünde:

```powershell
dotnet restore WebStok.slnx
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run dotnet-ef --tool-manifest dotnet-tools.json -- database update --project WebStok.DataAccess --startup-project WebStok.Web
dotnet user-secrets set "Bootstrap:AdminPassword" "<kendinize-ait-guclu-parola>" --project WebStok.Web
dotnet dev-certs https --trust
dotnet run --project WebStok.Web --launch-profile https
```

[Yerel uygulamayı açın](https://localhost:7196). İlk açılışta, veritabanında kullanıcı yoksa `admin` hesabı belirlediğiniz parolayla oluşturulur. Giriş için HTTPS profili kullanılmalıdır.

Veritabanı `WebStok.Web/App_Data/webstok.db` konumundadır. Yerel veritabanı, parolalar ve derleme çıktıları Git deposuna dahil edilmez. Mevcut verileriniz varsa kurulumdan önce yedek alın.

## Proje yapısı

| Proje | Sorumluluk |
| --- | --- |
| WebStok.Web | MVC ekranları, kimlik doğrulama ve yetkilendirme |
| WebStok.Business | İş kuralları, servisler ve doğrulama |
| WebStok.DataAccess | EF Core, SQLite ve migration dosyaları |
| WebStok.Domain | Varlıklar ve arayüzler |

## Doğrulama

```powershell
dotnet build WebStok.slnx
```

Bu depo geliştirme aşamasındadır. Kapsam ve önceki kontrol notları için [DEVELOPMENT.md](DEVELOPMENT.md) dosyasına bakın.
