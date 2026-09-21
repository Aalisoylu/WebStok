# WebStok geliştirme durumu

## Bu güncelleme
- Gerçek stok verileriyle genel bakış: kritik stok, ürün/depo sayısı, yaklaşan SKT ve son hareketler.
- Türkçe, mobil ekranlara uyarlanan yan menü; ortak form, tablo ve kart tasarımı.
- İade formu ve hareket geçmişinden iade bağlantısı.
- Tamamı iade edilmiş hareketlerde iade bağlantısının gizlenmesi.

## Doğrulama
C# ve Razor kaynakları .NET 10 derleyicisiyle hatasız doğrulandı. NuGet kullanıcı ayar klasörüne erişim sorunu nedeniyle normal solution restore/build tamamlanamadı.

Ayrı bir veritabanı kopyasında giriş, dashboard ve dokuz yönetim sayfası; geçerli iade, tekrar gönderimde mükerrer kayıt oluşmaması, fazla miktarın reddi, depo yetkisi ve antiforgery kontrolleri test edildi. Test kopyasında yerel HTTP için cookie ayarları geçici değiştirildi; projenin HTTPS ve güvenli cookie ayarları değiştirilmedi. Bu kontroller normal publish ve HTTPS testlerinin yerine geçmez.

Tarayıcıda son görsel doğrulama, yerel önizlemenin statik dosya/sunucu sorunları nedeniyle tamamlanmadı.

## Teslimden önce
- .NET 10 SDK ile normal restore, build ve HTTPS üzerinden uçtan uca doğrulama.
- Masaüstü ve mobil görünümün son kontrolü.
- Şartnameyle kapsam karşılaştırması: destek, izin onayları ve genel denetim logu modülleri henüz tamamlanmış değildir.
- Yayınlama ve kurulum dokümantasyonu.

## Çalıştırma
.NET 10 SDK kurulu ortamda proje kökünden:

```powershell
dotnet restore WebStok.slnx
dotnet build WebStok.slnx
dotnet run --project WebStok.Web --launch-profile https
```

Mevcut uygulama ASP.NET Core MVC ve SQLite kullanır. Veritabanı `WebStok.Web/App_Data/webstok.db` dosyasındadır. Veri içeren dosyayı koruyun.

## 21 Eylül 2026 arayüz yenilemesi

- İkonlu, operasyon ve tanım gruplarına ayrılmış yan menü.
- Yenilenmiş metrik kartları, açıklamalı hızlı işlemler ve geniş ekranlarda iki sütunlu özet.
- Ortak form/tablo stilleri ve mobil uyumlu yeni giriş ekranı.
- GitHub için .gitignore ve yerel kurulum README dosyası.
- .NET SDK 10.0.401 ile `dotnet build WebStok.slnx` başarılı: 0 hata, 0 uyarı. Önceki NuGet/derleme engeli gerekli ortam erişimiyle çözüldü.
- Canlı HTTPS görsel doğrulaması tamamlanamadı: bilgisayarda kullanılabilir geliştirme sertifikası bulunmuyor. Masaüstü/mobil tarayıcı ve HTTPS uçtan uca kontrolleri bekliyor.
