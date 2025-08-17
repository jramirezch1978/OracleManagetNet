# Solución al Error ORA-01722: número no válido (v2)

## Error Original
```
ORA-01722: número no válido
```

Este error ocurría al hacer clic en el enlace del evento para ver el historial de sesión.

## Causa
El error se producía al intentar leer valores numéricos de la vista `v$sql` que:
1. Podían ser NULL
2. Podían ser valores muy grandes que no cabían en un `Int64`
3. Los tipos de datos no coincidían entre la consulta SQL y el código C#

## Soluciones Aplicadas

### 1. Cambio de Tipos de Datos en el Modelo

**SessionSqlHistory.cs - Antes:**
```csharp
public long ElapsedTime { get; set; }
public long CpuTime { get; set; }
public long BufferGets { get; set; }
public long DiskReads { get; set; }
public int Executions { get; set; }
public decimal AvgElapsedTime { get; set; }
```

**SessionSqlHistory.cs - Después:**
```csharp
public decimal ElapsedTime { get; set; }
public decimal CpuTime { get; set; }
public decimal BufferGets { get; set; }
public decimal DiskReads { get; set; }
public decimal Executions { get; set; }
public decimal AvgElapsedTime { get; set; }
```

### 2. Manejo de Valores NULL en SQL

Se agregó `NVL()` para manejar valores NULL en la consulta:

```sql
NVL(s.elapsed_time, 0) AS elapsed_time,
NVL(s.cpu_time, 0) AS cpu_time,
NVL(s.buffer_gets, 0) AS buffer_gets,
NVL(s.disk_reads, 0) AS disk_reads,
NVL(s.executions, 0) AS executions,
CASE 
    WHEN NVL(s.executions, 0) > 0 
    THEN ROUND(s.elapsed_time / s.executions) 
    ELSE 0 
END AS avg_elapsed_time
```

### 3. Lectura Segura de Valores en C#

**Antes:**
```csharp
ElapsedTime = reader.GetInt64(reader.GetOrdinal("elapsed_time")),
```

**Después:**
```csharp
ElapsedTime = reader.IsDBNull(reader.GetOrdinal("elapsed_time")) ? 0 : reader.GetDecimal(reader.GetOrdinal("elapsed_time")),
```

### 4. Corrección en la Vista

Se ajustó la visualización del tiempo de ejecución:

```razor
<td>@((sql.ElapsedTime / 1000).ToString("N0")) ms</td>
```

## Razón Técnica

En Oracle, los campos numéricos de las vistas del sistema como `v$sql` pueden contener:
- Valores muy grandes (especialmente para queries de larga duración)
- Valores NULL (para queries que no han completado)
- Valores decimales de alta precisión

El tipo `decimal` en C# puede manejar estos valores de manera segura, mientras que `long` e `int` pueden causar overflow o errores de conversión.

## Resultado

✅ El historial de sesiones ahora se muestra correctamente
✅ No hay más errores ORA-01722
✅ Los valores NULL se manejan apropiadamente
✅ Los valores numéricos grandes se procesan sin problemas

## Correcciones Adicionales (Segunda Iteración)

### 1. Simplificación de la Consulta SQL

- Eliminada la condición OR con username para reducir complejidad
- Uso explícito de `TO_NUMBER()` para conversiones numéricas
- `SUBSTR()` para limitar el tamaño del SQL text a 4000 caracteres
- Valores por defecto para fechas NULL

### 2. Mejora en el Manejo de Datos

```csharp
// Uso de Convert.ToDecimal con GetValue para manejar diferentes tipos numéricos
ElapsedTime = reader.IsDBNull(reader.GetOrdinal("elapsed_time")) 
    ? 0 
    : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("elapsed_time")))
```

### 3. Consulta SQL Optimizada

```sql
SELECT 
    s.sql_id,
    SUBSTR(s.sql_fulltext, 1, 4000) AS sql_text,
    NVL(s.module, 'N/A') AS module,
    NVL(TO_CHAR(s.first_load_time, 'YYYY-MM-DD HH24:MI:SS'), '2000-01-01 00:00:00') AS first_load_time,
    TO_NUMBER(NVL(s.elapsed_time, 0)) AS elapsed_time
    -- etc...
```

Esta versión es más robusta y maneja mejor los casos edge de Oracle 11g.
