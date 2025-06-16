# GymMate

Aplicación MAUI para entrenamiento.

## CI

La compilación de Android requiere los siguientes secrets en el repositorio:

- `ANDROID_KEYSTORE_BASE64`
- `KEYSTORE_PASSWORD`
- `KEY_ALIAS`
- `KEY_PASSWORD`
- `GH_TOKEN`

Para generar un keystore de prueba puedes usar:

```bash
keytool -genkeypair -v -keystore my.keystore
```

TODO: subir secrets
