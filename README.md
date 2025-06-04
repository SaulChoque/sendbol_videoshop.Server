# Sendbol Videoshop Plataforma de Marketplace de Productos Digitales con C#, MongoDB y Redis

●	Integrantes:
○	Kevin Josué Alvarado Mamani
○	Saul Mijael Choquehuanca Huanca
○	Hernán Pérez Ovando
●	Docente: Lic. Celia Tarquino Peralta
●	Materia: INF 261/251
●	Gestión: 2025

Este proyecto está compuesto por dos repositorios principales:

- **Cliente Angular:** [sendbol_videoshop.client](https://github.com/SaulChoque/sendbol_videoshop.client)
- **Servidor ASP.NET Core:** [sendbol_videoshop.Server](https://github.com/SaulChoque/sendbol_videoshop.Server)

---

## Requisitos previos

- [Node.js](https://nodejs.org/) (recomendado v18+)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- [.NET 7 SDK](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/try/download/community)
- [Redis](https://redis.io/download)
- Git

---

## Clonar los repositorios

```bash
git clone https://github.com/SaulChoque/sendbol_videoshop.client.git
git clone https://github.com/SaulChoque/sendbol_videoshop.Server.git
```

---

## Configuración del servidor

1. Entra a la carpeta del servidor:

    ```bash
    cd sendbol_videoshop.Server
    ```

2. Revisa y ajusta la configuración de la base de datos en `appsettings.Development.json` si es necesario (por defecto usa MongoDB y Redis locales).

3. Restaura los paquetes NuGet:

    ```bash
    dotnet restore
    ```

4. Compila y ejecuta el servidor:

    ```bash
    dotnet build
    dotnet run
    ```

   El servidor estará disponible en `https://localhost:54993` (puerto configurable).

---

## Configuración del cliente

1. Entra a la carpeta del cliente:

    ```bash
    cd sendbol_videoshop.client
    ```

2. Instala las dependencias de Node.js:

    ```bash
    npm install
    ```

3. Ejecuta la aplicación Angular en modo desarrollo:

    ```bash
    ng serve
    ```

   El cliente estará disponible en `http://localhost:4200`.

---

## Notas adicionales

- Asegúrate de que MongoDB y Redis estén corriendo localmente antes de iniciar el servidor.
- El cliente está configurado para comunicarse con el backend en `https://localhost:54993` mediante proxy.
- Puedes modificar los puertos y las cadenas de conexión en los archivos de configuración según tus necesidades.

---

## Compilación para producción

### Cliente

```bash
ng build --configuration production
```

Los archivos generados estarán en la carpeta `docs/` (según configuración de `angular.json`).

### Servidor

```bash
dotnet publish -c Release
```

---
