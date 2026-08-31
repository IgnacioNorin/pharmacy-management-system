# Historial de cambios

El formato sigue, a grandes rasgos, [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [Sin publicar]

### Seguridad

- **Bitácora de acciones administrativas.** Nueva tabla `security_event`
  (migración `032`): una fila por operación sensible con quién la hizo, sobre qué
  y un resumen legible. Registra: cambios de permisos de un rol
  (`rol Empleado: +ventas.acceso, −reportes.acceso`), alta / renombre / baja de
  roles; alta / edición (con el cambio de rol) / baja de usuarios; edición de los
  datos de la tienda; edición de la configuración de alertas; y el alta / edición
  / baja de productos, categorías, clientes y proveedores. Por ahora se consulta
  directo en la base; una pantalla dedicada queda para más adelante. Los rastros
  existentes (`login_attempt`, historial de precios, historial de alertas) se
  mantienen aparte.
- **Límite de intentos de inicio de sesión.** Tras 5 intentos fallidos con el
  mismo documento en 15 minutos, la cuenta queda bloqueada temporalmente y el
  login informa cuántos minutos faltan para poder reintentar. El bloqueo se
  levanta solo al pasar la ventana sin más intentos, con un inicio de sesión
  correcto, o con un desbloqueo manual. Cada intento (ok o fallido) queda
  registrado en la tabla nueva `login_attempt` (migración `031`); las filas de
  más de 90 días se purgan solas.
- **Cambio de contraseña forzado en el primer ingreso.** La cuenta sembrada y
  todo usuario creado desde la pantalla de Usuarios nacen con
  `must_change_password`: al iniciar sesión aparece un diálogo obligatorio de
  cambio de contraseña que no se puede saltar. Además hay una opción "Cambiar
  contraseña" en la cabecera de la barra lateral, disponible para cualquier
  usuario. Largo mínimo de contraseña: 6 caracteres.
- **Acciones de administración por usuario.** En la lista de Usuarios, la columna
  "Acciones" abre un diálogo con las operaciones sobre esa cuenta (permiso
  `usuarios.gestionar`), separadas del formulario de alta/edición:
  - **Restablecer contraseña**: el sistema genera una contraseña temporal (el
    administrador no la elige ni necesita la actual), la muestra una vez y la
    copia al portapapeles; el usuario debe cambiarla al ingresar. Queda registro
    de qué administrador lo hizo.
  - **Desbloquear**: limpia el bloqueo por intentos fallidos.
  - **Suspender / Reactivar**: habilita o deshabilita el inicio de sesión de la
    cuenta sin tocar la contraseña. Es también la forma de deshacer una baja
    hecha por error. No se puede suspender al último Administrador General
    activo; queda auditado.
- **Script `Database/reset_admin_password.sql`.** Restablece la contraseña del
  último Administrador General cuando no queda ninguna otra cuenta que pueda
  hacerlo desde la aplicación.
- **Columna "Estado" en la lista de Usuarios.** Cada fila muestra si el usuario
  está *Activo*, *Inactivo* (suspendido o dado de baja) o *Bloqueado* (por
  intentos fallidos). La columna se actualiza sola al usar las acciones de esa
  pantalla.
- **Control de acceso más estricto (DEF-21/22/23).**
  - Sin una sesión iniciada, los controles de permiso ahora *deniegan* en vez de
    conceder (antes, un estado sin sesión abría toda la interfaz).
  - Registrar una venta y registrar una compra revalidan el permiso en el
    momento de la acción, no solo al abrir la pantalla.
  - Los permisos dejaron de quedar congelados hasta cerrar sesión: al volver a la
    ventana principal (por ejemplo, tras cerrar una pantalla) se re-resuelven el
    rol y los permisos desde la base. Si la cuenta fue desactivada o eliminada
    mientras estaba en uso, la sesión se cierra y vuelve al inicio de sesión.

### Corregido

- **Robustez ante datos incompletos (DEF-14/17/20/24).**
  - Un campo de cantidad vacío en la compra ya no lanza una excepción al validar
    el formulario: muestra el mensaje de campo requerido como el resto.
  - "Ver" en el centro de alertas, con un producto sin código, avisa en vez de
    fallar.
  - Los índices e ids ocultos y los combos de las pantallas de mantenimiento se
    leen de forma tolerante (un valor inesperado degrada a un valor por defecto,
    no cierra la operación).
  - Imprimir un comprobante con la ficha de tienda a medio completar ya no lanza
    una referencia nula: los campos faltantes salen vacíos.
- **El comprobante impreso en impresora normal ahora trae los datos fiscales
  (DEF-11).** El comprobante HTML (la ruta por defecto) incluye el bloque
  RECEPTOR de la factura (RUT, razón social, giro, dirección, comuna) y el
  desglose Neto / IVA / Exento, igual que el de impresora térmica. Una nota de
  crédito muestra la venta que anula.
- **El comprobante HTML escapa el contenido (DEF-12).** Los nombres de empresa,
  producto y receptor se codifican antes de insertarse en la plantilla, así que
  un producto llamado `Vitamina C & D` o con `<` ya no rompe el diseño.
- **El globo de alertas se actualiza al cerrar el centro de notificaciones
  (DEF-16).** Reconocer o silenciar una alerta ahora refresca el contador de
  inmediato, en vez de esperar al siguiente chequeo automático.
- **La numeración de comprobantes ya no se corta pasado el 999.999 (DEF-18).**
  Un folio de siete o más dígitos se guarda completo, sin colisionar con el
  índice único.
- **La configuración de alertas rechaza valores sin sentido (DEF-19).** Los días
  para "por vencer" van de 1 a 3650 y el stock crítico de 0 a 100.000; un valor
  negativo o enorme desactivaba la alerta en silencio.
- **Validación de teléfono (DEF-41).** Ya no rechaza números largos o con prefijo
  de país; admite dígitos y los separadores habituales (`+ - ( )` y espacios),
  de 6 a 20 caracteres.
- **Limpieza (DEF-25/26/27/34/36/39).** Se eliminó código muerto
  (`RawPrinterHelper`), la clave `.pfx` de firma (desactivada) del árbol de
  trabajo y del `.csproj`; los errores de impresión se registran en `error.log`
  en vez de `Console`; el temporizador de alertas fija su intervalo antes de
  arrancar; editar un producto borrado por debajo ahora informa el fallo; y
  agregar dos veces el mismo producto a una compra avisa en vez de no hacer nada.
- **Reconocer una alerta ya no pisa a otro usuario (DEF-38).** Si dos personas
  reconocen la misma alerta a la vez, solo el primero queda registrado; reconocer
  una alerta ya resuelta no hace nada.
- **La valorización del informe de productos excluye los dados de baja (DEF-40).**
  Los productos descontinuados siguen apareciendo como filas, pero ya no cuentan
  en los totales de valorización de inventario.
- **La venta vuelve a Boleta al terminar (DEF-31).** Tras registrar una venta,
  el tipo de documento se restablece a Boleta y el panel del receptor se oculta,
  en vez de arrancar la siguiente venta en modo Factura con los datos anteriores.

## [1.4.0] - 2026-08-30

### Agregado

- **Trazabilidad por lote (DEF-02 fase A).** Nueva tabla `product_lot`: una fila
  por partida recibida de un producto, con su cantidad restante, su vencimiento y
  su costo de compra. Cada línea de compra crea un lote; la venta descuenta de los
  lotes que vencen antes (FEFO); una nota de crédito devuelve las unidades como un
  lote sin fecha. `product.stock` pasa a ser la suma de los lotes; se hizo un lote
  inicial por cada producto que tenía stock (migración `026`).
- **Alertas de vencimiento por lote.** La alerta de "por vencer / vencido" ya no
  mira el campo único `product.date_expired`, sino los lotes: una alerta por
  producto, con el lote con stock que vence antes y su cantidad ("vence el
  dd/mm/yyyy (N u.)"). Así un lote nuevo con vencimiento lejano no puede apagar la
  alerta del stock viejo que sigue en góndola, y cuando ese stock se vende (FEFO
  vacía primero el lote más próximo) la alerta se apaga sola. Cierra DEF-02.
- **Valorización por lote y vista de lotes.** El reporte de productos suma una
  columna "Valor Stock (costo)" que valoriza cada lote a su propio costo de
  compra (`SUM(cantidad × costo)`), en vez de aplicar un único precio a todo el
  stock; el total del reporte también lo trae. En la pestaña Productos de Gestión,
  el botón "Ver lotes" abre una ventana con los lotes del producto seleccionado:
  cantidad, vencimiento y costo unitario por partida, con el total de unidades y
  el valor.
- **Pago mixto en la venta.** Una venta se puede cobrar con más de una forma de
  pago (por ejemplo, parte efectivo y parte tarjeta). Botón "Pago mixto…" junto
  al combo de forma de pago: abre un diálogo con un monto por método que debe
  sumar el total. El desglose se guarda en la tabla `sale_payment` (migración
  `025`); `sale.payment_method` pasa a ser el método "principal" (el de mayor
  monto). El comprobante lista cada forma de pago con su monto y solo muestra
  "PAGO CON" / "CAMBIO" cuando hubo efectivo. Una venta 100% con tarjeta o
  transferencia ya no exige ingresar "pagó con". El reporte de ventas muestra
  "Mixto" en esos casos. Las notas de crédito reparten el reintegro entre las
  mismas formas de pago de la venta original, así que el arqueo de caja descuenta
  de cada método lo que corresponde.
- **Arqueo de caja.** Nueva opción en la barra lateral (grupo Consulta), gateada
  por el permiso `caja.acceso` (roles Administrador General y Administrador).
  Muestra el período desde el último arqueo hasta ahora y, por cada forma de pago
  (Efectivo / Tarjeta / Transferencia), el total esperado según el sistema y un
  campo para el monto contado, con la diferencia por método y total. Se guarda un
  registro en `cash_count` / `cash_count_line` (migración `024`); las ventas no se
  modifican. Las notas de crédito, al tener monto negativo, se descuentan solas
  del esperado.
- **Forma de pago en la venta.** La pantalla de venta pide cómo se cobró
  (Efectivo / Tarjeta / Transferencia), un método por venta. Se guarda en
  `sale.payment_method` (migración `023`), sale en el comprobante y hay una
  columna "Forma de Pago" en el reporte de ventas. Las ventas anteriores quedan
  en Efectivo.
- **Subtotal por línea en el reporte de compras.** Cada línea de producto ahora
  trae una columna "Subtotal" (cantidad × precio de compra), y el total del
  reporte suma esa columna. Antes solo se veía la cantidad y el precio unitario.
- **Desglose de IVA en las compras.** El comprobante de compra ahora guarda su
  neto, su IVA, su parte exenta y la tasa aplicada (migración `030`). El precio de
  compra se ingresa con IVA incluido, igual que en la venta, y el neto se descuenta
  hacia atrás (`neto = total / 1,19`); las líneas de productos exentos
  (`product.tax_affected` en 0) van a la parte exenta. La pantalla de compra
  muestra "Neto / IVA" arriba del monto total, y el reporte de compras suma las
  columnas "Neto", "IVA" y "Exento" del período (útil para el crédito fiscal). Las
  compras anteriores se rellenaron como afectas al 19 %.

### Cambiado

- **Los clientes se separaron a su propia tabla.** Hasta ahora un cliente era una
  fila de `person` (la misma tabla de los usuarios) con el rol "Cliente". Eso
  dejaba los hashes de contraseña junto a los datos de clientes y obligaba a cada
  consulta de clientes a excluir esa columna a mano. Ahora hay una tabla `client`
  independiente, sin contraseña ni rol; `person` queda solo para los usuarios que
  inician sesión. La migración `029` mueve los clientes conservando sus ids
  (`sale.client_id` pasa a referenciar `client`), quita las columnas fiscales de
  `person` y retira el rol "Cliente". Sin cambios visibles en las pantallas de
  Clientes, el selector de cliente de la venta ni el filtro de los reportes.
- **El sistema maneja únicamente pesos chilenos (CLP).** Se quitó por completo la
  moneda configurable y el preset de país. Todo el dinero —ventas, compras,
  reportes, comprobantes, arqueo de caja, valorización de stock— se formatea como
  CLP: símbolo `$`, punto de miles y **sin decimales** (`$2.000.000`), porque el
  peso no tiene unidad menor. Cada monto que el usuario tipea o que el sistema
  calcula se redondea al peso entero (medio hacia arriba). Se eliminaron las
  columnas `store.currency_culture` y `store.country_code` (migración `028`) y los
  combos de moneda y de preset de país de la pantalla de tienda. La validación del
  RUT del receptor de factura queda fija (antes dependía del preset). La tasa de
  IVA sigue siendo configurable en la pantalla de tienda (19 por defecto). Los
  tipos de documento de venta quedan fijos en Boleta/Factura y la numeración de
  folio sigue siendo "Factura vs. resto".
- **Baja lógica coherente.** Un proveedor referenciado por compras se da de baja
  lógicamente (`status = 0`) en vez de fallar con "revise los datos"
  (`sp_delete_supplier`, migración `022`). Los clientes dados de baja dejan de
  aparecer en la pantalla de Clientes, en el selector de la venta y en el filtro
  de cliente de los reportes. Al editar un producto cuya categoría fue dada de
  baja, el combo ahora incluye esa categoría, así que no se reasigna en silencio.
- **Base de datos caída: aviso claro en vez de pantalla vacía.** Antes, si SQL
  Server no respondía al abrir una grilla, esta quedaba vacía sin ningún aviso.
  Ahora los repositorios distinguen "no hay datos" de "no hay base" y la
  aplicación muestra "No se pudo conectar con la base de datos. Verifique que el
  servidor esté disponible e intente nuevamente." sin cerrarse: la operación
  falló, pero se puede reintentar. Cubre Productos, Clientes, Proveedores,
  Reportes, Usuarios, Roles y Tienda. El ciclo de alertas en segundo plano
  ignora el fallo y reintenta en el siguiente tick, sin abrir diálogos.
  (Antes solo estaba cubierto el inicio de sesión, la venta y la compra.)

### Rendimiento

- **Imprimir un comprobante** ya no carga todo el historial de ventas (dos
  veces): `SaleRepository.GetById` / `GetDetailsBySaleId` traen solo esa venta.
- **La pantalla de venta** deja de cargar el catálogo completo en cada escaneo y
  en cada agregado al carrito: `ProductRepository.GetSellableByCode` /
  `GetSellableById`.
- **El selector de clientes, la pantalla de Clientes y el filtro de los
  reportes** dejan de cargar todas las personas (usuarios y sus hashes
  incluidos): `PersonRepository.ListClients` trae solo clientes activos, sin la
  columna de contraseña.
- **Grillas de Productos, Clientes y Proveedores paginadas en el servidor.** Antes
  cada una traía la tabla completa y filtraba las filas en memoria. Ahora la base
  devuelve una página de 50 filas por vez (`OFFSET/FETCH` + conteo total en una sola
  consulta), con una barra de navegación (`|<  <  >  >|` y "Página X de Y · N
  registros") debajo de cada grilla. El buscador pasó a ser una consulta al
  servidor: el texto se compara contra código/nombre/descripción (Productos),
  nombre/documento/razón social/correo (Clientes) o razón social/documento/correo
  (Proveedores), y la búsqueda también se pagina. Se dispara con Enter o el botón
  buscar, no al tipear.

### Corregido

- **No se podían guardar los datos de la tienda** cuando la razón social o la
  dirección eran un poco largas. Las columnas de texto de `store` eran
  `varchar(50)`: un valor más largo hacía fallar el `UPDATE` con "string would be
  truncated", que la capa de datos convertía en un "No se pudo guardar los datos,
  revise los datos" sin más explicación. Migración `027`: `company_name` pasa a
  150, `address` a 200, `email` a 120; la validación de la pantalla acompaña esos
  límites.
- **Montos en dólares en la compra y la venta.** Al tipear `2000000` el sistema
  mostraba `200.000,00` (formato con dos decimales de otra cultura) en vez de
  `$2.000.000`. Con el paso a CLP puro, el ingreso y la muestra de dinero usan
  siempre el formato chileno de peso entero, sin decimales.

### Infraestructura

- **Integración continua (GitHub Actions).** El flujo `.github/workflows/ci.yml`
  compila la solución con MSBuild y corre las pruebas de unidad, negocio,
  presentación y UI en cada push a `main` o a una rama `refactor/**` y en cada
  pull request hacia `main`. Las pruebas de integración, que necesitan un SQL
  Server con el esquema aplicado, se siguen corriendo en local.

## [1.3.0] - 2026-08-29

### Agregado

- **Pantalla de Precios (Gestión).** El precio de venta se fija en una pestaña
  propia, gateada por `productos.editar_precios`: dos vistas —productos en stock
  *por liberar* y productos *en comercialización* con costo, precio y margen %—,
  un formulario para asignar o cambiar el precio (con motivo), y un panel con el
  historial de precios del producto seleccionado.
- **Estado de comercialización** (`product.is_released`). Un producto creado y
  comprado tiene stock pero no se vende hasta que se lo **libera** desde la
  pantalla de Precios. La venta solo ofrece productos liberados. Se puede
  **retirar** un producto de la venta sin darlo de baja.
- **Historial de precios** (`product_price_history`): una fila por cada
  liberación, cambio de precio o retiro, con el costo del momento, el usuario y
  un motivo.
- **Costo promedio ponderado** (`product.average_cost`), recalculado en cada
  compra. **Costo congelado por línea de venta** (`sale_detail.unit_cost`), para
  que el margen de una venta pasada no cambie si el costo cambia después.
- **Manejador global de excepciones** (`Program.cs` + `StartupError`): un error
  no capturado ya no cierra la aplicación con el diálogo de .NET; un problema de
  `ConnectionStrings.config` o de base de datos muestra un mensaje propio.
- **`Database/create_app_login.sql`**: login `pharmacy_app` con privilegios
  mínimos sobre `PharmacyDB` — la aplicación deja de necesitar `sa`.

### Cambiado

- **La compra ya no fija el precio de venta.** `frmPurchase` no pide "Precio
  Venta"; `PurchaseRepository.Register` solo mueve stock y costo.
- **La administración de roles no puede quedar inaccesible** (migración `017`):
  no se puede quitar `roles.gestionar` del último rol que lo tiene, ni por
  `sp_set_role_permissions` ni al borrar el rol.
- **"Base de datos caída" deja de confundirse con "no hay datos"** en inicio de
  sesión, venta y compra: esas rutas relanzan un error de conexión como
  `DataUnavailableException` y muestran "No se pudo conectar con la base de
  datos" en vez de "no se encontraron coincidencias" / "verifique el stock".
- **El vencimiento de cada lote comprado se guarda** (`purchase_detail.date_expired`,
  migración `018`); la fecha de vencimiento del producto **solo se adelanta** en
  la compra, así un lote nuevo no apaga la alerta del stock viejo.
- **No se puede registrar dos veces la misma factura de proveedor** (índice
  único `UX_purchase_supplier_document`, migración `020`).

### Corregido

- El comprobante impreso y los reportes ya no arrastran el precio de venta desde
  la compra.
- Corrección de la errata "No se econtraron coincidencias" → "encontraron".

### Migraciones

- `017` a `021`. Además, ejecutar una vez `Database/create_app_login.sql` para
  crear el login de la aplicación con privilegios mínimos.

## [1.2.0] - 2026-08-29

### Agregado

- **Permisos por rol.** El acceso deja de estar fijo por rol: cada permiso
  (`seccion.accion`, 30 en total) se puede asignar o quitar de un rol, y se
  pueden crear roles nuevos además de los cuatro de siempre.
  - Nueva pantalla **Roles y permisos** (barra lateral → Gestión, solo la abre
    quien tiene `roles.gestionar`, que de fábrica es únicamente el
    Administrador General): árbol de permisos por rol (cada sección con su
    permiso de acceso y, debajo, sus permisos internos; tildar un permiso
    arrastra a sus padres, destildar uno limpia a sus hijos), alta / renombrar
    / eliminar de roles personalizados. Los cuatro roles del sistema se pueden
    re-permisar pero no renombrar ni eliminar; un rol con usuarios asignados
    no se puede eliminar.
  - La barra lateral y las pestañas de Gestión y de Reportes se muestran según
    los permisos del usuario. Cada acción sensible (crear/editar/eliminar en
    cada sección, editar la tienda, configurar alertas, reconocer/silenciar
    alertas) se valida en el presenter y además deshabilita el botón.
  - Los reportes se controlan **por tipo**: `reportes.acceso` abre la pantalla
    y, adentro, ver y exportar son permisos separados para ventas, compras,
    productos e historial de alertas (`reportes.ventas`,
    `reportes.ventas.exportar`, …). Cada pestaña y cada botón de exportar se
    muestra según su permiso.
  - La pantalla de Inicio oculta las tarjetas y los accesos rápidos de compras
    y de stock a los roles sin esos permisos; los handlers de navegación de la
    ventana principal validan el permiso antes de abrir la sección.
  - El combo de rol en la pantalla de Usuarios se arma desde `person_type`, así
    que muestra también los roles personalizados.
  - **Rol Administrador General protegido.** Un usuario que no es Administrador
    General no puede crear, editar, eliminar ni asignar ese rol, y nadie puede
    eliminar ni degradar al último Administrador General activo (dejaría el
    sistema sin quien administre roles y tienda). Se valida en el presenter de
    Usuarios y en `sp_delete_person` / `sp_update_person`.
  - Migraciones `Database/Migrations/002_user_permissions.sql`,
    `003_role_admin.sql`, `004_report_permissions.sql` y
    `005_protect_admin_general.sql`. Tras aplicarlas el comportamiento es
    idéntico al de la 1.1.0 hasta que un administrador edite un rol.

- **Modelo de IVA (nivel 2, fase A).** La venta ahora guarda su desglose:
  neto + IVA + exento = total. Los precios se siguen ingresando con IVA
  incluido; el neto se calcula hacia atrás para la parte afecta. La tasa vive
  en `store.default_tax_rate` (19 por defecto, pero es configurable, no una
  constante), y cada producto tiene `tax_affected` (afecto / exento). El ticket
  muestra NETO / IVA (tasa%) / EXENTO, y el reporte de ventas suma esas tres
  columnas. En Gestión de productos hay un checkbox "Afecto a IVA" y en Tienda
  un campo "Tasa IVA (%)". Migración `007_tax_model.sql` (rellena las ventas
  existentes asumiendo 100% afecto al 19%).
- **Tipo de documento boleta / factura (nivel 2, fase B).** La pantalla de venta
  ofrece Boleta y Factura (antes solo "Factura", fijo). El correlativo es
  independiente por tipo (`seq_folio_boleta` / `seq_folio_factura`, reemplazan a
  `seq_sale_folio`), y el número es único por `(tipo, número)` — boleta 000001 y
  factura 000001 pueden coexistir. El tipo por defecto se configura en Tienda
  (`store.default_document_type`). Migración `008_document_types.sql` (reinicia
  el correlativo de factura pasado el número más alto ya emitido).
- **Datos del receptor en la factura (nivel 2, fase C).** Al elegir "Factura" en
  la pantalla de venta se despliega un panel que pide RUT, razón social, giro,
  dirección y comuna del receptor. El RUT se valida con dígito verificador
  chileno (módulo 11, aislado en `ChileanRutValidator` — el resto de los
  documentos sigue con el validador neutro). Se guardan en `sale.recipient_*` y
  aparecen en el ticket bajo "RECEPTOR". Migración `009_factura_recipient.sql`.
- **Nota de crédito / anular venta (nivel 2, fase D).** Botón "Nota de crédito"
  en la pantalla de venta (permiso `ventas.nota_credito`, de fábrica solo los
  dos roles de administrador): se busca un comprobante por tipo y número, y se
  emite una nota de crédito que lo reversa — un `sale` nuevo con montos en
  negativo y `reference_id` al original, el stock de cada línea devuelto, todo
  en una transacción. Los reportes suman todo, así que la NC netea sola; no se
  puede anular dos veces ni anular una nota de crédito. Migración
  `010_credit_note.sql` (`sale.reference_id`, `seq_folio_nota_credito`, permiso).
- **Enganche de emisión fiscal (nivel 2, fase E).** Los comprobantes que emite el
  sistema **no son documentos tributarios**: se numeran con la secuencia local y
  no se envía nada a ninguna autoridad. Se agregó el punto de integración para
  cuando haga falta emitir DTE: interfaz `IFiscalDocumentIssuer`, implementación
  por defecto `LocalSequenceIssuer` (deja el comprobante como interno) y columnas
  `sale.fiscal_status` / `fiscal_track_id` / `fiscal_barcode`. Al registrar una
  venta, `SaleService` le pasa el comprobante al emisor y guarda lo que resuelva.
  Conectar un proveedor DTE es escribir una implementación nueva de la interfaz y
  cambiar el registro en `CompositionRoot`. Migración `011_fiscal_document_hook.sql`.
- **Preset de país.** La tienda recuerda un preset (`store.country_code`, en
  blanco = genérico) que agrupa los valores por defecto de un país: tasa de IVA,
  cultura de moneda y el esquema con que se valida el documento del receptor de
  una factura. Chile es el único preset concreto por ahora (IVA 19, `es-CL`,
  RUT módulo 11); el genérico no hace supuestos nacionales: valida el documento
  del receptor solo por formato (`DocumentValidator`), no con módulo 11. La
  validación al emitir una factura deja de asumir RUT chileno y usa el esquema
  del preset. La moneda por defecto (cuando no hay ninguna configurada) pasa de
  `es-EC` (Ecuador, ya fuera del proyecto) a `en-US`. En Gestión de tienda hay
  un combo "País / preset": elegir uno concreto pre-llena la tasa de IVA y la
  moneda (siguen editándose a mano). Migraciones `015_store_country_preset.sql`
  y `016_neutral_default_currency.sql`.
- **Ficha fiscal del cliente y vínculo venta - cliente.** La ficha de cliente
  suma razón social, giro / actividad, comuna / localidad, email y una marca
  "es empresa" (razón social y giro obligatorios si está marcada). La venta
  queda vinculada al cliente elegido (`sale.client_id`, en blanco para
  consumidor final), y al emitir una factura los datos del receptor se
  precargan desde ese cliente. El informe de ventas suma un filtro "Cliente"
  (junto al rango de fechas) que usa ese vínculo. Migraciones
  `012_client_fiscal_profile.sql` (`person.business_name` / `activity` /
  `commune` / `email` / `is_company`, `sale.client_id`) y
  `013_person_update_fiscal_profile.sql` (`sp_update_person` escribe los campos
  nuevos).

### Cambiado

- **Reportes: datos tipados en vez de una tabla de textos.** El presenter de
  Reportes ahora entrega cada informe como una definición de columnas
  (`encabezado` + tipo + selector) más los datos crudos (`decimal` / `DateTime`
  sin formatear) y una fila de totales de la misma forma; el formateo pasó a la
  vista.
- **Reportes: los totales salen del área ordenable.** En Ventas y Compras, los
  totales dejan de ser una fila del grid (ordenar una columna ya no los
  desacomoda) y pasan a una franja de solo lectura debajo de la grilla. La
  exportación los sigue incluyendo como fila "Total:".
- **Reportes: exportar ahora ofrece Excel, CSV y PDF.** Un solo botón
  "Exportar" por pestaña; el formato se elige en el diálogo de guardado. El
  Excel pasa a tener celdas tipadas de verdad (montos y cantidades como número
  con formato, fechas como fecha), fila de totales en negrita, filtro
  automático y encabezado fijo. El CSV usa el separador de la configuración
  regional, UTF-8 con BOM y números planos (re-importables). El PDF sale en A4
  horizontal con título, tabla y número de página. Si el archivo de destino
  está abierto en otra aplicación, el mensaje lo dice.
- **Reportes: el rango de fechas arranca en el mes actual.** Los selectores
  "Desde" de Ventas, Compras e Historial de alertas empiezan en el primer día
  del mes (antes arrancaban en hoy y una consulta recién abierta salía vacía).
- **Reportes: nombre de archivo exportado** con formato `Reporte_yyyyMMdd_HHmmss`
  (ordena cronológicamente en el explorador).
- **Reportes: nombres de columna neutros y con acentos.** En el informe de
  ventas, "CI Vendedor" / "CI Cliente" pasan a "Documento Vendedor" / "Documento
  Cliente"; se corrigen acentos en los encabezados ("Código", "Descripción",
  "Categoría", "Número Documento", "Razón Social").
- **Reporte de ventas: un solo par de columnas para cliente/receptor.**
  "Documento Cliente" y "Cliente / Razón Social" muestran el cliente en una
  boleta y el receptor en una factura (el repositorio hace el `COALESCE`). En
  una factura los datos del receptor ya no se guardan además en
  `document_client` / `name_client`: se acabó la duplicación.
- **Reporte de productos: fila de totales.** Total de unidades en stock y
  valorización del inventario a costo (Σ stock × precio de compra) y a precio de
  venta (Σ stock × precio de venta), en la franja bajo la grilla y en la
  exportación.

### Interno

- **Runner de migraciones (`PharmacySystem.DbMigrator`).** Proyecto de consola
  con DbUp que aplica las migraciones pendientes en orden, una transacción por
  script, anotando lo aplicado en `dbo.SchemaVersions`. Reemplaza correrlas a
  mano con `sqlcmd` / SSMS (así se coló el `QUOTED_IDENTIFIER OFF` en la 013).
  Sobre una base ya existente sin journal, registra las migraciones actuales
  como aplicadas sin re-ejecutarlas. Cadena de conexión por argumento, variable
  `PHARMACY_DB_CONNECTION` o `ConnectionStrings.config`.
- Reportes: exportadores `IReportExporter` (CSV / XLSX / PDF) en
  `PharmacySystem.Presentation`, cada uno recorriendo la misma
  `ReportDefinition`. Los 4 handlers de exportación de `frmReport` quedan en un
  único método que elige el exportador por la extensión del archivo.
  `PharmacySystem.Presentation` suma dos paquetes: `ClosedXML` (ya usado por el
  proyecto WinForms) y `PDFsharp-MigraDoc-GDI` (100% administrado, sin binarios
  nativos).
- Reportes: los totales de ventas y las columnas de línea de compras se calculan
  a partir de las filas ya consultadas. Se eliminaron `SumAmountReceived` /
  `SumChangeAmount` (ventas) y `GetTotalPurchasePrice` / `GetTotalQuantity` /
  `GetTotalSalesPrice` / `GetSubTotal` (compras) de servicios y repositorios.
  El informe de ventas pasa de cinco consultas a una; el de compras, de cinco a
  dos (se conserva `GetTotalAmount`, el total de cabecera con su test de
  regresión, y `SumTotalPay`, que usa el tablero de Inicio).
- Se quitó la referencia muerta a Entity Framework 6 del proyecto WinForms
  (no se usaba y hacía fallar el build en un clon sin la carpeta `packages`).
- Se eliminaron seis procedimientos almacenados de alta/edición
  (`sp_create_person` / `_product` / `_supplier`, `sp_update_product` /
  `_supplier` / `_category`): solo hacían un chequeo de duplicado con
  `IF NOT EXISTS` que ya cubren los índices únicos. Los repositorios ahora
  ejecutan el `INSERT` / `UPDATE` directo y mapean el error 2601/2627 a
  "duplicado". Migración `006_drop_redundant_sps.sql`.

## [1.1.0]

### Corregido

- **Monto "Paga con" con monedas sin símbolo `$`:** `CultureInfoHelper`
  reconocía un importe ya formateado buscando `$` fijo; con `es-PE` (Sol, `S/`)
  u otra moneda sin `$` eso fallaba al reconvertir el texto a número. Ahora usa
  el símbolo real de la moneda activa.
- **Alta de cliente / usuario:** `Register` devuelve el id de la fila nueva en
  vez de un booleano. Antes, un cliente o usuario recién creado quedaba con
  `Id = 0` en la grilla y volver a guardarlo desde esa fila lo registraba de
  nuevo (duplicado). Ahora la fila lleva el id real y se puede editar.
- **Venta e inventario:** el descuento de stock se ejecuta dentro de la misma
  transacción que registra la venta y su detalle, con guardia
  `stock >= cantidad` por línea. Si una línea no tiene stock suficiente se
  revierte toda la venta; ya no queda stock descontado sin venta registrada ni
  descuentos parciales sin *rollback*.
- **Número de comprobante de venta:** se genera con una secuencia
  (`dbo.seq_sale_folio`) dentro de la transacción, con índice único. Antes se
  calculaba con `COUNT(*) + 1`, que asignaba el mismo número a ventas
  simultáneas y se repetía tras un borrado.
- **Reporte de compras:** el total de compras dejaba de ser correcto cuando una
  compra tenía más de una línea de detalle (sumaba el monto de la cabecera una
  vez por línea).
- **Instalación nueva:** una base recién creada no podía guardar el perfil de la
  tienda ni los umbrales de alerta. Ahora el esquema siembra ambas filas de
  configuración y los procedimientos y repositorios las crean si faltan.
- **Baja de productos:** un producto que solo había disparado alertas (sin
  compras ni ventas) no se podía eliminar. `sp_delete_product` ahora contempla
  `product_alert_history`.
- **Edición de usuarios:** editar un usuario sin tocar la contraseña ya no la
  reescribe; dejar el campo en blanco conserva la actual.
- **Reportes por fecha:** los filtros de fecha usan `DateTime` tipado y un rango
  *sargable* en lugar de texto sensible a la configuración regional.
- Truncado silencioso de `name`/`description` de producto en
  `sp_create_product` / `sp_update_product` (parámetros alineados al ancho real
  de las columnas).

### Agregado

- Baja lógica de usuarios (`sp_delete_person`): una persona referenciada por
  ventas, compras o alertas reconocidas se desactiva (`status = 0`) en vez de
  fallar el borrado. El inicio de sesión rechaza usuarios con `status = 0`.
- Índices únicos en documento de persona/proveedor, código de producto y
  descripción de categoría; índices en las columnas de clave foránea.
- Rotación de `error.log` al superar 5 MB y escritura segura entre procesos.
- `Database/Migrations/001_upgrade_to_1_1_0.sql` para actualizar una base ya
  desplegada sin recrearla.
- `DEPLOY.md` y `deploy/package.ps1` para armar un paquete de distribución.

### Cambiado

- Todas las columnas de importes pasan de `decimal(10,2)` a `decimal(18,2)`
  para admitir monedas de bajo valor sin desbordar.
- Opciones de base `ANSI_NULLS`, `ANSI_PADDING`, `ANSI_WARNINGS`, `ARITHABORT`,
  `QUOTED_IDENTIFIER` y `CONCAT_NULL_YIELDS_NULL` en `ON`.

## [1.0.0]

- Primera versión: punto de venta, compras, inventario con alertas de stock y
  vencimiento, clientes, proveedores, usuarios con roles y reportes.
