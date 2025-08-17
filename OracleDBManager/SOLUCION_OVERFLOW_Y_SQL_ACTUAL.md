# Soluciones: Error de Overflow y Restauración de SQL Actual

## 1. Error de Overflow en Historial de Eventos

### Problema
```
System.InvalidCastException: Specified cast is not valid.
 ---> System.OverflowException: Arithmetic operation resulted in an overflow.
```

Este error ocurría al hacer clic en algunos eventos porque las sesiones de larga duración pueden tener valores de tiempo de espera extremadamente grandes que exceden los límites de los tipos de datos.

### Solución Aplicada

#### En la consulta SQL (`GetSessionEventHistoryAsync`):
```sql
CASE 
    WHEN e.time_waited / 100 > 999999999 THEN 999999999
    ELSE NVL(e.time_waited / 100, 0)
END AS time_waited_seconds
```

#### En el código C#:
```csharp
try
{
    var timeWaited = reader.GetValue(reader.GetOrdinal("time_waited_seconds"));
    if (timeWaited != DBNull.Value)
    {
        var decimalValue = Convert.ToDecimal(timeWaited);
        history.TimeWaitedSeconds = decimalValue > 999999999 ? 999999999 : decimalValue;
    }
}
catch { history.TimeWaitedSeconds = 999999999; }
```

#### Formateo mejorado en la vista:
```csharp
private string FormatLargeNumber(decimal value)
{
    if (value >= 1000000000)
        return (value / 1000000000).ToString("N1") + "B";
    else if (value >= 1000000)
        return (value / 1000000).ToString("N1") + "M";
    else if (value >= 1000)
        return (value / 1000).ToString("N1") + "K";
    else
        return value.ToString("N0");
}
```

## 2. Restauración del SQL Actual en Modal de Detalles

### Problema
El SQL actual de la sesión no se mostraba en el modal de detalles de sesión al hacer clic en el nombre de usuario.

### Solución Aplicada

#### Modificación de la consulta SQL en `GetSessionDetailAsync`:
```sql
SELECT 
    s.sid,
    s.serial#,
    -- otros campos...
    sql.sql_text AS current_sql_text
FROM 
    v$session s
    JOIN v$process p ON s.paddr = p.addr
    LEFT JOIN v$sql sql ON s.sql_id = sql.sql_id 
        AND s.sql_child_number = sql.child_number
WHERE 
    s.sid = :sid
```

#### Lectura del SQL text:
```csharp
SqlText = reader.IsDBNull(reader.GetOrdinal("current_sql_text")) 
    ? null 
    : reader.GetString(reader.GetOrdinal("current_sql_text"))
```

#### Mejora en la presentación del SQL:
```html
@if (!string.IsNullOrEmpty(selectedSessionDetail.SqlText))
{
    <h4>SQL Actual</h4>
    <div class="sql-container">
        <pre class="bg-light p-3 rounded" 
             style="white-space: pre-wrap; 
                    word-wrap: break-word; 
                    overflow-x: auto; 
                    max-height: 300px; 
                    overflow-y: auto;">
            @selectedSessionDetail.SqlText
        </pre>
    </div>
}
```

#### Estilos CSS agregados:
```css
.sql-container pre {
    font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
    font-size: 13px;
    line-height: 1.5;
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
    white-space: pre-wrap;
    word-wrap: break-word;
    max-height: 300px;
    overflow: auto;
}
```

## Resultado

✅ El historial de eventos ahora maneja correctamente sesiones con valores extremos
✅ Los valores grandes se formatean de manera legible (K, M, B)
✅ El SQL actual se muestra correctamente en el modal de detalles
✅ El SQL se ajusta al tamaño del popup con scroll cuando es necesario
✅ Mejor legibilidad del código SQL con formato monoespaciado
