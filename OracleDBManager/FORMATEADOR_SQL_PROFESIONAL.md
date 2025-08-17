# Formateador SQL Profesional

## Características Implementadas

### 1. **Formato SQL Estructurado**

El SQL ahora se muestra formateado profesionalmente con:

- **Cláusulas en líneas separadas**: SELECT, FROM, WHERE, GROUP BY, HAVING, ORDER BY
- **Campos alineados**: Máximo 4 campos por línea en SELECT para mejor legibilidad
- **JOINs indentados**: Con ON en su propia línea indentada
- **Condiciones WHERE organizadas**: AND/OR en líneas separadas con indentación

### 2. **Resaltado de Sintaxis**

Colores aplicados a diferentes elementos SQL:

- **Palabras clave** (azul, negrita): SELECT, FROM, WHERE, etc.
- **Funciones** (púrpura): COUNT(), TO_CHAR(), NVL(), etc.
- **Strings** (verde): Valores entre comillas simples
- **Números** (rojo): Valores numéricos
- **Operadores** (cyan, negrita): =, <>, AND, OR, etc.
- **Comentarios** (gris, cursiva): -- comentarios y /* bloques */

### 3. **Mejoras Visuales**

- **Fuente monoespaciada**: Consolas/Monaco para mejor alineación
- **Fondo suave**: Color gris claro (#f8f9fa)
- **Borde redondeado**: Para un aspecto más moderno
- **Scroll personalizado**: Barras de desplazamiento estilizadas
- **Altura máxima**: 400px con scroll automático para SQLs largos

### 4. **Ejemplo de Transformación**

**Antes:**
```sql
SELECT s.sid, s.serial#, s.username, s.osuser, s.machine, s.terminal, s.program, s.module, s.action, s.client_info, s.logon_time, s.status, s.state, s.wait_class, s.wait_time, s.seconds_in_wait, s.sql_id, s.sql_child_number, s.prev_sql_id, p.spid AS os_process_id, p.pga_used_mem, p.pga_alloc_mem, sql.sql_text AS current_sql_text FROM v$session s JOIN v$process p ON s.paddr = p.addr LEFT JOIN v$sql sql ON s.sql_id = sql.sql_id
```

**Después:**
```sql
SELECT
    s.sid, s.serial#, s.username, s.osuser,
    s.machine, s.terminal, s.program, s.module,
    s.action, s.client_info, s.logon_time, s.status,
    s.state, s.wait_class, s.wait_time, s.seconds_in_wait,
    s.sql_id, s.sql_child_number, s.prev_sql_id, p.spid AS os_process_id,
    p.pga_used_mem, p.pga_alloc_mem, sql.sql_text AS current_sql_text
FROM
    v$session s
    JOIN v$process p
        ON s.paddr = p.addr
    LEFT JOIN v$sql sql
        ON s.sql_id = sql.sql_id
```

### 5. **Implementación Técnica**

#### JavaScript (App.razor):
- **formatSql()**: Formatea el SQL con reglas de indentación
- **highlightSql()**: Aplica clases CSS para resaltado de sintaxis
- Manejo inteligente de paréntesis y subconsultas
- Preservación de strings y comentarios

#### CSS (oracle-em-style.css):
- Clases específicas para cada tipo de elemento SQL
- Diseño responsivo con scrollbars personalizados
- Tipografía optimizada para código

#### Blazor (Sessions.razor):
- Llamadas asíncronas a JavaScript para formatear
- Renderizado como MarkupString para HTML
- Aplicado tanto en detalles de sesión como en historial SQL

### 6. **Ventajas**

✅ **Legibilidad mejorada**: SQL complejo ahora es fácil de leer
✅ **Análisis rápido**: Colores ayudan a identificar elementos
✅ **Profesional**: Aspecto similar a herramientas SQL profesionales
✅ **Rendimiento**: Formateo rápido en el cliente
✅ **Mantenibilidad**: Fácil de extender con nuevas reglas

### 7. **Casos de Uso**

- Modal de detalles de sesión: SQL actual formateado
- Historial de consultas: Todas las SQL históricas formateadas
- Expansible a otras áreas donde se muestre SQL
