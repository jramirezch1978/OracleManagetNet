# Solución al Error ORA-00904 en la Página de Sesiones

## Error Original
```
Error al obtener las sesiones: ORA-00904: "C"."CLIENT_INFO": identificador no válido
```

## Causa
La consulta SQL intentaba acceder a una columna `CLIENT_INFO` desde la vista `v$session_connect_info` con el alias `c`, pero esta columna no existe en esa vista.

## Solución Aplicada

### Modificación en LockRepository.cs

Se corrigió la consulta SQL eliminando la referencia incorrecta:

**Antes:**
```sql
SELECT 
    ...
    c.client_info AS connection_info
FROM 
    v$session s
    LEFT JOIN v$process p ON s.paddr = p.addr
    LEFT JOIN v$session_connect_info c ON s.sid = c.sid
```

**Después:**
```sql
SELECT 
    ...
    -- Se eliminó la línea c.client_info AS connection_info
FROM 
    v$session s
    LEFT JOIN v$process p ON s.paddr = p.addr
    -- Se eliminó el JOIN con v$session_connect_info ya que no era necesario
```

### Explicación Técnica

1. La columna `client_info` ya existe en la vista `v$session` y se estaba obteniendo correctamente desde ahí
2. La vista `v$session_connect_info` contiene información sobre la conexión pero no tiene una columna `client_info`
3. Como no necesitábamos ninguna información adicional de `v$session_connect_info`, se eliminó el JOIN completo

## Resultado

La página de sesiones ahora funciona correctamente y muestra todas las sesiones activas de la base de datos sin errores.
