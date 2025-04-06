#  StoreVisitTracker API

Mağaza ziyaretlerini takip etmek, mağaza ürünlerini ve fotoğrafları yönetmek için geliştirilen RESTful bir .NET 8 Web API projesidir. Redis ile cache işlemleri yapılır, JWT ile kimlik doğrulama uygulanır.

<hr>

##  İçindekiler

- [Özellikler](#özellikler)
- [Kullanılan Teknolojiler](#kullanılan-teknolojiler)
- [Gereksinimler](#gereksinimler)
- [Hızlı Başlangıç](#hızlı-başlangıç)
- [Dosya ve Dizinler](#dosya-ve-dizinler)





##  Özellikler

-  JWT ile giriş yapan kullanıcıya özel işlem
-  Admin yetkisi gerektiren endpoint’ler
-  Redis ile ürün ve mağaza listeleme cache’i
-  Swagger UI üzerinden tüm API'yi test edebilme
-  Kullanıcıya ait ziyaret başlatma / tamamlama
-  Fotoğraf yükleme ve listeleme

<hr>


## Kullanılan Teknolojiler

- .NET 8
- Entity Framework Core
- MySQL
- Redis
- Swagger / OpenAPI
- JWT (JSON Web Token)
- xUnit & Moq (Unit Testler için)

<hr>


##  Kurulum ve Çalıştırma Adımları

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL (örneğin XAMPP ile)
- Redis (localhost:6379)
- Visual Studio Code veya Visual Studio 2022+



### Hızlı Başlangıç



- [En son sürümü indir](https://github.com/MuratKoseli/StoreVisitTracker/archive/refs/heads/main.zip)
- Depoyu klonla : `git clone https://github.com/MuratKoseli/StoreVisitTracker.git`


## Dosya ve Dizinler  

İndirilen dosyanın içinde aşağıdaki dosyalar ve dizinler bulunmaktadır.

<summary>İndirme İçeriği</summary>

```StoreVisitTracker/
├── StoreVisitTracker.Api/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── StoreVisitTracker.Application/
│   └── (Uygulama mantığı ve DTO'lar)
├── StoreVisitTracker.Domain/
│   └── Entities/
├── StoreVisitTracker.Infrastructure/
│   └── Db/
│       └── AppDbContext.cs
├── StoreVisitTracker.Tests/
│   ├── Controllers/
│   └── Unit test dosyaları
├── StoreVisitTracker.sln
└── README.md
```
