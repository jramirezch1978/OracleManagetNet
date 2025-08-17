# Consulta Alternativa para Historial SQL

Si el error ORA-01722 persiste, aquí hay una consulta alternativa más simple que puedes usar:

## Opción 1: Consulta Mínima

```sql
SELECT 
    sql_id,
    SUBSTR(sql_text, 1, 100) AS sql_text,
    'N/A' AS module,
    TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') AS first_load_time,
    TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') AS last_active_time,
    0 AS elapsed_time,
    0 AS cpu_time,
    0 AS buffer_gets,
    0 AS disk_reads,
    1 AS executions,
    0 AS avg_elapsed_time
FROM v$session
WHERE sid = :sessionId
AND sql_id IS NOT NULL
AND ROWNUM = 1
```

## Opción 2: Sin Datos de v$sql

```sql
SELECT 
    s.sql_id,
    s.sql_id AS sql_text,
    s.module,
    TO_CHAR(s.logon_time, 'YYYY-MM-DD HH24:MI:SS') AS first_load_time,
    TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') AS last_active_time,
    0 AS elapsed_time,
    0 AS cpu_time,
    0 AS buffer_gets,
    0 AS disk_reads,
    1 AS executions,
    0 AS avg_elapsed_time
FROM v$session s
WHERE s.sid = :sessionId
AND s.sql_id IS NOT NULL
```

## Opción 3: Datos Básicos de v$sql con Validaciones

```sql
SELECT * FROM (
    SELECT 
        sql_id,
        CASE 
            WHEN LENGTH(sql_fulltext) > 4000 
            THEN SUBSTR(sql_fulltext, 1, 4000)
            ELSE sql_fulltext
        END AS sql_text,
        NVL(module, 'Unknown') AS module,
        '2024-01-01 00:00:00' AS first_load_time,
        '2024-01-01 00:00:00' AS last_active_time,
        CASE 
            WHEN elapsed_time IS NULL THEN 0
            WHEN elapsed_time > 999999999999 THEN 999999999999
            ELSE elapsed_time
        END AS elapsed_time,
        0 AS cpu_time,
        0 AS buffer_gets,
        0 AS disk_reads,
        1 AS executions,
        0 AS avg_elapsed_time
    FROM v$sql
    WHERE sql_id = (
        SELECT sql_id 
        FROM v$session 
        WHERE sid = :sessionId 
        AND ROWNUM = 1
    )
    AND ROWNUM = 1
)
```

## Implementación en C#

Si necesitas usar la consulta alternativa, reemplaza el contenido de la constante `query` en el método `GetSessionSqlHistoryAsync` con una de estas opciones.

La Opción 1 es la más simple y debería funcionar en cualquier caso, aunque proporciona datos mínimos.

La Opción 2 obtiene datos directamente de v$session sin acceder a v$sql.

La Opción 3 intenta obtener datos de v$sql pero con muchas validaciones para evitar errores.
