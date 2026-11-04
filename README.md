# Aerolínea VIP · Grupo 08

Sistema web monolítico para la compra, reserva y administración de pasajes de una aerolínea VIP con tres aeronaves Embraer Phenom 300E de 7 asientos.

**Integrantes:** Carlos Andrés Carrasco Novoa · Joselin Liseth Villegas Chicaiza
**Asignatura:** Desarrollo Web para Integración de Tecnologías · ESPE

## Tecnologías

- ASP.NET Framework 4.8.1 con MVC y vistas Razor
- Bootstrap 5 y jQuery (validaciones del cliente)
- ASP.NET Identity (usuarios, roles e inicio de sesión)
- Entity Framework 6 (Database First)
- SQL Server

## Cómo ejecutar el proyecto

1. **Base de datos.** En SQL Server ejecute, en este orden:
   - `BaseDatos/Script_BD_AerolineaVIP_Unificado.sql`: crea las bases `AerolineaVIP` y `AerolineaVIP_Identity` con los datos de ejemplo.
   - `BaseDatos/Datos_Vuelos_Proximos_Dias.sql`: genera vuelos para los próximos 14 días desde la fecha en que se ejecuta.
2. **Conexión.** `Web.config` usa `Integrated Security=True` (autenticación de Windows) con el servidor local. Si su instancia de SQL Server tiene otro nombre, cambie `Data Source` en las dos cadenas de conexión.
3. **Abrir y compilar.** Clone el repositorio en una ruta corta (por ejemplo `C:\G8\`), abra `Progra_03.slnx` en Visual Studio, restaure los paquetes NuGet (clic derecho en la solución → *Restaurar paquetes NuGet*) y compile.
4. **Ejecutar** con F5. Al iniciar, `Global.asax.cs` crea por código los roles y los usuarios de prueba si todavía no existen.
5. Para tener vuelos en fechas más lejanas, ingrese como **superusuario** → Administración → Vuelos → *Generar vuelos*.

## Usuarios de prueba

| Usuario | Contraseña | Roles | Qué puede hacer |
|---|---|---|---|
| `superusuario` | `Super2026*` | view, create, edit, delete | Toda la administración; generar vuelos; crear usuarios y asignar perfiles |
| `administrador` | `Admin2026*` | view, edit, delete | Ver, editar y eliminar en la administración; reportes; cancelar reservas y cambiar asientos. No crea registros ni usuarios |
| `counter.uio` | `Counter2026*` | Counter | Atención al cliente: buscar un cliente por usuario o cédula y comprar, pagar, cancelar, cambiar asiento y hacer check-in a su nombre |
| `cliente` | `Cliente2026*` | Cliente | Buscar vuelos, reservar, pagar, cancelar sus reservas, check-in y comprobantes |
| `pasajero` | `Pasajero2026*` | Pasajero | Ver los viajes donde aparece su cédula y hacer el check-in de su asiento |

En el registro público se puede elegir **Cliente** (compra pasajes) o **Pasajero** (ve los viajes que otra persona le compró por su cédula y hace su check-in, pero no puede comprar). Los perfiles Administrador y Counter solo los crea el superusuario en Administración → Usuarios.

## Flujo principal

1. **Buscar vuelo.** En *Inicio* o *Vuelos* elija origen, destino, fecha y número de pasajeros (1 a 7). El calendario de *Vuelos* solo habilita los días con vuelos.
   - Quito – Guayaquil: martes, jueves y sábado.
   - Quito – Manta: lunes, miércoles y viernes.
   - Quito – Galápagos: domingo, con escala en Guayaquil.
2. **Resultados y disponibilidad.** Elija un vuelo o un itinerario y revise el mapa de asientos: verde disponible, morado reservado, rojo ocupado.
3. **Seleccionar asientos.** Pide iniciar sesión como cliente. Elija un asiento por pasajero.
4. **Datos de los pasajeros.** Documento (cédula ecuatoriana o pasaporte), nombres, apellidos y datos de contacto.
5. **Confirmar la reserva.** Acepte las condiciones y elija *Confirmar reserva (pagar después)* o *Confirmar y pagar*. La reserva queda PENDIENTE durante 60 minutos; si no se paga, expira y los asientos se liberan.
6. **Pagar.** Complete los datos de la tarjeta y confirme en la ventana de confirmación. Con el pago aprobado la reserva pasa a CONFIRMADA y se emite la factura.
7. **Check-in.** Disponible desde 48 horas antes de la salida hasta la hora del vuelo, con el código de reserva (RV + 8 dígitos) y el documento del pasajero. Al terminar se muestra la tarjeta de embarque.
8. **Comprobantes.** Consulta e impresión de las facturas.

## Datos para probar

- **Pago:** los pagos son simulados, sin cobros reales (alcance académico del proyecto). Cualquier tarjeta con el formato correcto se aprueba; una tarjeta terminada en `0000` se rechaza para probar ese caso.
  - Visa aprobada: `4111111111111111`, vencimiento `12/28`, CVV `123`
  - Visa rechazada: `4111111111110000`, vencimiento `12/28`, CVV `123`
- **Cédulas válidas para pasajeros nuevos:** `1723450001`, `0912340007`, `1309870002`
- **Vuelos especiales de los datos de ejemplo:** el sábado 07/11/2026 a las 19:00 (Quito → Guayaquil) no tiene aeronave asignada y no se vende; el viernes 06/11/2026 a las 19:00 (Quito → Manta) está cancelado.

## Estructura del proyecto

| Carpeta | Contenido |
|---|---|
| `Controllers/` | Un controlador por módulo y los CRUD generados con scaffolding |
| `Models/` | Modelo de Entity Framework, métodos de cálculo (`*_Calculos.cs`), validaciones (`Metadatos.cs`) y ViewModels |
| `Services/` | Lógica compartida de reservas y generación de vuelos |
| `Filters/` | Filtro de sesión |
| `Views/` | Vistas Razor agrupadas por controlador |
| `BaseDatos/` | Scripts de SQL Server |
