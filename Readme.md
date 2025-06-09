## Nombre del formato: **SimpleConfig**

> Un formato de configuración sencillo, humano y flexible.

# Especificación del DSL SimpleConfig

## 1. Comentarios
- Una línea que comienza con `#` o `//` se considera un comentario.
- Los comentarios son ignorados por el parser.

```plaintext
# Comentario de ejemplo
// Otro comentario
```

---

## 2. Asignaciones individuales
- Para valores sueltos en la raíz se utiliza `SET nombre = valor`.
- `SET` es opcional dentro de un bloque.

```plaintext
SET title = "SimpleConfig Example"
SET version = 1.0
```

---

## 3. Bloques
- Los bloques se declaran con `NOMBRE_BLOQUE:`.
- También pueden tener un identificador: `NOMBRE_BLOQUE identificador:`.

```plaintext
OWNER:
  name = "John Doe"

SERVER alpha:
  ip = "10.0.0.1"
  role = frontend
```

Reglas:
- Dentro de un bloque se permiten claves con valores o sub-bloques.

---

## 4. Sangría y agrupación
- La sangría es opcional, usada solo para claridad visual.
- Un bloque finaliza al iniciar otro bloque o al terminar el archivo.

```plaintext
OWNER:
name = "Jane"
dob = 1980-01-01
```

---

## 5. Tipos de valores

| Tipo     | Ejemplo                               | Notas                                     |
|----------|---------------------------------------|-------------------------------------------|
| String   | "texto"                               | Comillas necesarias si hay espacios       |
| Número  | 123, 3.14                              | Enteros o decimales                      |
| Booleano | true, false                           | Siempre en minúsculas                   |
| Fecha    | 1979-05-27T07:32:00-08:00             | ISO 8601                                  |
| Null     | null                                  | Literal "null"                           |

---

## 6. Listas
- Listas se separan por comas.
- No requiere corchetes `[]`.

```plaintext
ports = 8000,8001,8002
names = "Tom","Jerry","Spike"
```

---

## 7. Sub-bloques (anidamiento)
- Un sub-bloque se declara dentro de un bloque superior con `nombre_subbloque:`.

```plaintext
DATABASE:
  temp_targets:
    cpu = 79.5
    case = 72.0
```

---

## 8. Reglas adicionales
- **Nombres**: letras, números, `_` y `.` permitidos.
- **Identificadores**: interpretados como atributos si existen (`SERVER alpha:`).
- **Errores**: cualquier línea fuera de un contexto válido es error de sintaxis.

---

## Ejemplo completo

```plaintext
# Configuración de ejemplo

SET title = "Mi Aplicación"
SET version = 1.2

OWNER:
  name = "Jhobanny"
  dob = 1979-05-27T07:32:00-08:00

DATABASE:
  enabled = true
  ports = 5432,5433
  temp_targets:
    cpu = 79.5
    case = 72.0

SERVER frontend:
  ip = "10.0.0.1"
  role = "web"

SERVER backend:
  ip = "10.0.0.2"
  role = "api"
```

---

