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

## CI iOS

Para las builds de iOS y su distribución en TestFlight se deben definir los siguientes secrets en GitHub:

- `APPLE_ID`
- `APPLE_TEAM_ID`
- `FASTLANE_SESSION`
- `FASTLANE_APPLE_APPLICATION_SPECIFIC_PASSWORD`
- `MATCH_GIT_PASSWORD`
- `GH_TOKEN`
- `CERTIFICATES_BASE64`
- `PROVISIONING_BASE64`

La cookie `FASTLANE_SESSION` se obtiene siguiendo la [guía de fastlane spaceauth](https://docs.fastlane.tools/actions/spaceauth/).
