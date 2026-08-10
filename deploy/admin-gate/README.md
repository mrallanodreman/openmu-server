# OpenMU Admin Gate

Basic-authenticated reverse proxy for the OpenMU AdminPanel. The upstream
OpenMU service stays private; this service is the only public admin entrypoint.

Credentials are supplied only through Railway variables:

- `ADMIN_USER`
- `ADMIN_PASSWORD`
