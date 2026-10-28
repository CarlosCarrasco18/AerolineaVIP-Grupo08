# Sistema web de compra y reserva de pasajes – Aerolínea VIP
Primera Entrega · Grupo 08

## Integrantes
- Carlos Andres Carrasco Novoa
- Joselin Liseth Villegas Chicaiza

## Tecnologías
ASP.NET Framework 4.8.1 · MVC · Razor · Bootstrap 5 · jQuery · ASP.NET Identity · Entity Framework 6 (Database First) · SQL Server

## Estructura del repositorio
| Carpeta | Contenido |
|---|---|
| `src/Progra_03/` | Proyecto ASP.NET MVC (solución `Progra_03.slnx`) |
| `database/` | Scripts SQL de la base de datos |
| `docs/` | Documentación de la entrega (PDF, wireframes aplicados y políticas) |

---

## 1. Cómo ejecutar el proyecto

1. Abrir **SQL Server Management Studio** y ejecutar completo (F5) `database/Script_BD_AerolineaVIP_Unificado.sql`.
   Crea las dos bases de datos: `AerolineaVIP` (dominio) y `AerolineaVIP_Identity` (usuarios y roles).
2. *(Opcional)* Ejecutar `database/Datos_Vuelos_Proximos_Dias.sql` para crear vuelos en los próximos 14 días.
   Sirve para probar el **check-in**, que solo se habilita 48 horas antes del vuelo.
3. Revisar en `src/Progra_03/Web.config` las dos cadenas de conexión:
   - `DefaultConnection` → `AerolineaVIP_Identity`
   - `AerolineaVIPEntities` → `AerolineaVIP`

   Ambas usan el servidor local con el usuario `sa` y la contraseña `123`.
4. Abrir `src/Progra_03/Progra_03.slnx` en Visual Studio.
5. Menú **Compilar → Recompilar solución** (descarga los paquetes NuGet) y ejecutar con **F5**.
6. Al iniciar, `Global.asax.cs` crea automáticamente los roles y los usuarios de la tabla siguiente.

> Si al ejecutar por primera vez aparece *"No se puede encontrar una parte de la ruta de acceso… bin\roslyn\csc.exe"*, usar **Compilar → Recompilar solución** y volver a ejecutar.

---

## 2. Usuarios creados en `Global.asax.cs`

| Usuario | Contraseña | Rol(es) | Qué puede hacer |
|---|---|---|---|
| `superusuario` | `Super2026*` | view, create, edit, delete | Acceso completo a **Administración**: ver, crear, editar y eliminar en todos los módulos |
| `administrador` | `Admin2026*` | edit, delete | Editar y eliminar registros de administración (no tiene permiso de ver listados ni de crear) |
| `counter.uio` | `Counter2026*` | view | Consultar listados, reservas, usuarios y reportes; asistir el check-in |
| `cliente` | `Cliente2026*` | Cliente | Buscar vuelos, reservar, pagar, ver **Mis reservas** y **Comprobantes**, cancelar |
| `pasajero` | `Pasajero2026*` | Pasajero | Ver **Mis viajes** (documento 0910000008) y realizar el check-in |

**Registro público:** en *Registrarse* solo se puede elegir el tipo de cuenta **Cliente** o **Pasajero**. Los roles de administración no se ofrecen y el servidor rechaza cualquier otro valor.

---

## 3. Recorrido sugerido para probar

**Compra de un pasaje (usuario `cliente`)**
1. *Vuelos* → elegir origen, destino y fecha → **Buscar**.
2. **Ver disponibilidad** → elegir asientos en el mapa:
   🟢 verde = disponible · 🔴 rojo = ocupado · 🟣 morado = reservado.
3. Completar los datos de cada pasajero (cédula ecuatoriana válida o pasaporte).
4. **Confirmar reserva**. La reserva queda *PENDIENTE* durante 60 minutos.
5. **Pagar** con datos de prueba:

   | Marca | Número | CVV | Vencimiento |
   |---|---|---|---|
   | Visa | `4111111111111111` | `123` | cualquier mes futuro, p. ej. `12/28` |
   | Mastercard | `5555555555554444` | `123` | `12/28` |
   | American Express | `378282246310005` | `1234` | `12/28` |

6. Se emite el comprobante y la reserva pasa a *CONFIRMADA*.

**Check-in (usuario `pasajero` o `cliente`)**
*Mis reservas / Mis viajes* → reserva confirmada → **Check-in** (habilitado desde 48 horas antes del vuelo).

**Administración (usuario `superusuario`)**
Menú *Administración* → Vuelos e itinerarios · Aeronaves · Usuarios, pasajeros y reservas · Reportes.

---

## 4. Scripts de apoyo (`database/`)

| Archivo | Uso |
|---|---|
| `Script_BD_AerolineaVIP_Unificado.sql` | Crea las dos bases de datos con sus tablas y datos iniciales |
| `Datos_Vuelos_Proximos_Dias.sql` | Agrega vuelos para los próximos días (para probar check-in) |
| `Reiniciar_Usuarios_Identity.sql` | Borra los usuarios de Identity; al volver a ejecutar la aplicación se crean de nuevo. Detener la aplicación antes de usarlo |

---

## Estado
Versión `v1.0-primera-entrega` · 28/10/2026 · Grupo 08
