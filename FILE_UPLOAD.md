# Subida de archivos

La página principal permite seleccionar un archivo de cualquier tipo (hasta 20 MB), ver su nombre y tamaño y enviarlo con **Subir archivo**. El botón muestra **Subiendo…** durante el envío. La selección y la subida básica también funcionan sin JavaScript.

Los archivos se guardan como datos binarios en el contenedor `default`, con el nombre original del archivo, sin las rutas que pudiera incluir el cliente. No se sobrescriben blobs existentes: si el nombre ya existe, la página pide renombrar el archivo. También se aceptan archivos vacíos.

La aplicación mantiene `AZURE_STORAGEBLOB_RESOURCEENDPOINT`, las credenciales registradas en `Startup`, Service Connector, las dependencias, .NET 9 y el pipeline actuales. Abrir o recargar la página ya no escribe el blob de demostración `HelloWorld`. No se necesita crear recursos adicionales.

## Verificación local

```powershell
dotnet restore WebStorageSample.csproj
dotnet build WebStorageSample.csproj --configuration Release --no-restore
dotnet publish WebStorageSample.csproj --configuration Release --no-build --output ./publish
```

Para subir desde desarrollo local, se mantiene la configuración existente: endpoint en `.env` y autenticación con Azure CLI. En producción se usan las credenciales de Service Connector existentes. Los errores técnicos se registran en el servidor; la página muestra mensajes en español.

## Prueba después del despliegue

1. Abrir la Web App, seleccionar un archivo de prueba y pulsar **Subir archivo**.
2. Confirmar el mensaje de éxito y encontrar el archivo en el contenedor `default`. Descargarlo y comparar su contenido con el original, especialmente para imágenes, PDF o archivos binarios.
3. Recargar la página: no debe volver a subir el archivo.
4. Volver a subir el mismo nombre: debe aparecer el aviso de duplicado y conservarse el contenido anterior.
5. Probar un nombre con espacios o acentos y un archivo vacío.
6. Probar sin archivo y con un archivo mayor de 20 MB: debe impedirse el envío. El servidor también valida el tamaño si se omite la validación del navegador.

El límite total de petición es 21 MB para permitir el formulario multipart. Peticiones que superen ese límite o el del servidor web pueden ser rechazadas antes de llegar a la página. La subida real, el mensaje de éxito y la detección de duplicados deben verificarse en Azure con las credenciales y permisos actuales.
