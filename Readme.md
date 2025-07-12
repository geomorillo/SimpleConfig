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

## 2. Asignaciones de Clave-Valor
- La sintaxis principal para definir datos es `clave = valor`.
- Esta sintaxis se utiliza tanto en la raíz del documento como dentro de los bloques.

```plaintext
title = "SimpleConfig Example"
version = 1.0
```

---

## 3. Bloques
- Los bloques se declaran con `NOMBRE_BLOQUE:`.
- También pueden tener un identificador opcional: `NOMBRE_BLOQUE identificador:`.

Cuando se declaran múltiples bloques con el mismo `NOMBRE_BLOQUE` pero diferentes identificadores (como `SERVER frontend` y `SERVER backend`), el parser los agrupa bajo un único objeto principal (`SERVER`), usando cada identificador como una clave. Esto permite definir múltiples instancias de un mismo tipo de objeto de forma organizada.

```plaintext
## Bloque simple (crea un objeto "OWNER")
OWNER:
  name = "John Doe"

## Bloques con identificador (se agrupan bajo un objeto "SERVER")
SERVER frontend:
  ip = "10.0.0.1"
  role = "web"

SERVER backend:
  ip = "10.0.0.2"
  role = "api"
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
|----------|---------------------------------------|------------------------------------------|
| String   | "cualquier texto"                     | Las comillas dobles (`"`) son siempre obligatorias para los valores de tipo string. |
| Número   | 123, 3.14                             | Enteros o decimales. No usan comillas.   |
| Booleano | true, false                           | Siempre en minúsculas                   |
| Fecha    | 1979-05-27T07:32:00-08:00             | ISO 8601                                  |
| Null     | null                                  | Literal "null"                           |

---

## 6. Listas
- Las listas son secuencias de valores separados por comas (`,`).
- El espacio en blanco alrededor de cada valor (antes y después de la coma) es ignorado.
- Si un valor en la lista debe contener una coma o espacios que no deben ser ignorados, **debe** estar encerrado entre comillas dobles (`"`).

```plaintext
# Lista de números (sencillo)
ports = 8000, 8001, 8002

# Lista de strings (siempre entre comillas)
# Se interpreta como: ["admin", "editor", "viewer"]
roles = "admin", "editor", "viewer"

# Lista de strings que contienen comas internas
# Se interpreta como: ["manzana,pera", "Naranja limón"]
frutas = "manzana,pera", "Naranja limón"

# Lista mixta
data = 1, "procesado", true, 12.5
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

title = "Mi Aplicación"
version = 1.2

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
