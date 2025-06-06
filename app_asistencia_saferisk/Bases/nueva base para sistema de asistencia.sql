-- ===========================================
-- BASE DE DATOS DE ASISTENCIA MEJORADA
-- ===========================================

-- 1. Crear base de datos y seleccionar su uso
CREATE DATABASE app_asistencia_db;
GO
USE app_asistencia_db;
GO

-- ===========================================
-- 2. Catálogo de roles
-- ===========================================
CREATE TABLE rol (
    rol_id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    estado INT NOT NULL DEFAULT 1
);
GO
-- ===========================================
-- Inserts de roles
-- ===========================================
INSERT INTO rol (nombre) VALUES
('Gerencia'),           -- 1
('Supervisor de Área'), -- 2
('Empleado'),           -- 3
('Sistemas'),           -- 4
('Contabilidad'),       -- 5
('RRHH'),               -- 6
('Administrador');      -- 7


-- ===========================================
-- 3. Usuarios
-- ===========================================
CREATE TABLE usuario (
    usuario_id INT IDENTITY(1,1) PRIMARY KEY,
    rol_id INT NOT NULL,
    supervisor_id INT NULL,
	nombres NVARCHAR(250) NOT NULL, 
    ci NVARCHAR(20) NOT NULL,
    pass NVARCHAR(MAX) NOT NULL,
    estado INT NOT NULL DEFAULT 1,
    creado_en DATETIME DEFAULT GETDATE(),
    actualizado_en DATETIME NULL,
	
    CONSTRAINT FK_usuario_rol_id FOREIGN KEY (rol_id) REFERENCES rol(rol_id),
	CONSTRAINT FK_usuario_supervisor FOREIGN KEY (supervisor_id) REFERENCES usuario(usuario_id)
);
GO
-- ===========================================
-- Insert Usuarios
-- ===========================================
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (1, NULL, 'Jorge Jaramillo', '170000000', 'jjaramillo');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (4, 4, 'Fernando Chanatasig', '1753666872', 'fchanatasig');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (4, 4, 'Kimberly Vasquez', '1725906414', 'kvasquez');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (2, NULL, 'Wilson Zambrano', 'wzambrano', 'wzambrano');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (2, NULL , 'ROCA MOREIRA GISELLA MABEL', '1722567052', 'xlx4mIxO');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'CAJIAO VIZCAINO CARLOS ALBERTO', '1712908365', 'PnD0bwPq');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'SANTIANA RUIZ KERLLY BRIGGITTE', '931120786', 'aNKXe4rF');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'ZAMBRANO SERRANO CRISTIAN PAUL', '503215311', 'ImnL6TxE');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (5, NULL, 'LUZURIAGA AULLA XIMENA ALEXANDRA', '1718384918', '3WKK0w4G');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (2, NULL, 'VELANDIA CARO DIANA CAROLINA', '1759673872', 'FuHCZ0Zl');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'RAMIREZ RAMOS JAVIER ALEJANDRO', '1758471088', 'VrQgKQgD');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'PINTO LUCAS ANDRES SEBASTIAN', '1753987906', 'Q6nxWQaX');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 10, 'AYALA FLORES ANDREA BERENISSE', '1751017250', '6I5Tmod7');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'VELASTEGUI VILLALBA LIDA JANINE', '1709878977', 'ZIy7nCtN');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, NULL, 'REASCOS SALAZAR OMAR SANTIAGO', '1714672811', 'mRkvuZ5R');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 10, 'PINCAY ESCOBAR SANTIAGO JACINTO', '1751198407', 'xynbdyFS');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 10, 'TUFIÑO HINOJOSA STHEFANI DAYANA', '1722903182', 'k5tkQWH4');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'TITUAÑA PACHECO INGRID NICOLE', '1754391355', 'hU7Zlale');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'YUQUILEMA TENE ANGELO RODRIGO', '1718513847', 'G6oj1NIo');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'VASQUEZ MORALES KARLA LIZETH', '1725906406', 'Wr8c5PFw');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'SOLIS VISCARRA JOSELINE PAOLA', '1750153478', 'JfpPL6Nx');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'ACHIG VISTIN STEFANIE XIOMARA', '1754039137', 'RpHSt7dv');
INSERT INTO usuario(rol_id,supervisor_id,nombres,ci,pass) VALUES (3, 5, 'MARIA REINOSO', '1718562471', '05tA10hS');



-- ===========================================
-- 4. Jornadas (una por día por usuario)
-- ===========================================
CREATE TABLE jornada (
    jornada_id INT IDENTITY(1,1) PRIMARY KEY,
    usuario_id INT NOT NULL,
    fecha DATE NOT NULL,
    tipo_jornada VARCHAR(20) NOT NULL,    -- 'oficina', 'remoto', 'mixto', etc.
    hora_inicio TIME(7) NULL,
    hora_fin TIME(7) NULL,
    observaciones NVARCHAR(255) NULL,
    estado VARCHAR(15) NOT NULL DEFAULT 'abierta', -- 'abierta', 'cerrada', etc.
    creado_en DATETIME DEFAULT GETDATE(),
    actualizado_en DATETIME NULL,
    CONSTRAINT FK_jornada_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario(usuario_id)
);
GO

-- Índice único: Un usuario no puede tener dos jornadas el mismo día
CREATE UNIQUE INDEX idx_jornada_usuario_fecha ON jornada(usuario_id, fecha);
GO

-- ===========================================
-- 5. Tipos de evento (catálogo)
-- ===========================================
CREATE TABLE tipo_evento (
    tipo_evento_id INT IDENTITY(1,1) PRIMARY KEY,
    codigo VARCHAR(30) NOT NULL UNIQUE,          -- 'entrada', 'salida', etc.
    descripcion NVARCHAR(100) NOT NULL,
    estado INT NOT NULL DEFAULT 1
);
GO

-- ===========================================
-- Insert tipo de evento 
-- ===========================================

INSERT INTO tipo_evento (codigo, descripcion) VALUES
('entrada',         'Entrada'),
('salida',          'Salida'),
('almuerzo_inicio', 'Inicio de almuerzo'),
('almuerzo_fin',    'Fin de almuerzo'),
('break_inicio',    'Inicio de break'),
('break_fin',       'Fin de break'),
('permiso',         'Permiso especial'),
('reunion',         'Reunión'),
('visita',          'Visita a cliente'),
('remoto',          'Cambio a trabajo remoto'),
('oficina',         'Cambio a trabajo en oficina'),
('otro',            'Otro');

-- ===========================================
-- 6. Eventos de jornada
-- ===========================================
CREATE TABLE evento (
    evento_id INT IDENTITY(1,1) PRIMARY KEY,
    jornada_id INT NOT NULL,
    tipo_evento_id INT NOT NULL,
    observaciones NVARCHAR(255) NULL,
    dispositivo NVARCHAR(100) NULL,
    ip_registro VARCHAR(45) NULL,
    latitud DECIMAL(10, 6) NULL,
    longitud DECIMAL(10, 6) NULL,
    creado_en DATETIME DEFAULT GETDATE(),
    hora_registro TIME(7) NULL,
    CONSTRAINT FK_evento_jornada_id FOREIGN KEY (jornada_id) REFERENCES jornada(jornada_id) ON DELETE CASCADE,
    CONSTRAINT FK_evento_tipo_evento_id FOREIGN KEY (tipo_evento_id) REFERENCES tipo_evento(tipo_evento_id)
);
GO

-- Índice para búsquedas rápidas de eventos por jornada
CREATE INDEX idx_evento_jornada_id ON evento(jornada_id);
GO


-- ===========================================
-- BASE LISTA PARA PRODUCCIÓN Y ESCALABLE
-- ===========================================

-- ===========================================
-- SP: Validar Usuario (Login)
-- ===========================================
/*
Valida usuario por CI y contraseña.
Devuelve usuario_id, nombres, rol_id y supervisor_id si el usuario está activo.
Usar siempre hasheo de contraseñas en producción (esto es solo ejemplo).
*/

CREATE PROCEDURE sp_validar_usuario
    @ci NVARCHAR(20),
    @pass NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        usuario_id,
        nombres,
        rol_id,
        supervisor_id,
        estado
    FROM usuario
    WHERE ci = @ci
      AND pass = @pass
      AND estado = 1;
    -- Si no retorna filas, usuario y clave no coinciden o el usuario está inactivo.
END
GO

-- ===========================================
-- SP: Registrar Jornada
-- ===========================================
/*
Registra el inicio de jornada para un usuario en una fecha.
Valida que el usuario esté activo, que el tipo de jornada sea válido y que no exista ya una jornada en esa fecha.
Devuelve el id de la nueva jornada.
*/
CREATE PROCEDURE sp_registrar_jornada
    @usuario_id INT,
    @fecha DATE,
    @tipo_jornada VARCHAR(20),
    @observaciones NVARCHAR(255) = NULL,
    @jornada_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM usuario WHERE usuario_id = @usuario_id AND estado = 1)
    BEGIN
        RAISERROR('Usuario no existe o está inactivo.', 16, 1);
        RETURN;
    END

    IF @tipo_jornada NOT IN ('oficina','remoto','mixto')
    BEGIN
        RAISERROR('Tipo de jornada no válido.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM jornada WHERE usuario_id = @usuario_id AND fecha = @fecha)
    BEGIN
        RAISERROR('Ya existe una jornada para este usuario en esta fecha.', 16, 1);
        RETURN;
    END

    INSERT INTO jornada (usuario_id, fecha, tipo_jornada, hora_inicio, observaciones)
    VALUES (@usuario_id, @fecha, @tipo_jornada, CAST(GETDATE() AS TIME), @observaciones);

    SET @jornada_id = SCOPE_IDENTITY();
    SELECT 'OK' AS estado, @jornada_id AS jornada_id;
END
GO

-- ===========================================
-- SP: Registrar Evento en Jornada
-- ===========================================
/*
Registra un evento (entrada, salida, break, etc.) en una jornada.
Valida que la jornada esté abierta, que el tipo de evento exista y no duplicar eventos consecutivos iguales.
Devuelve el id del nuevo evento.
*/
CREATE PROCEDURE sp_registrar_evento
    @jornada_id INT,
    @tipo_evento_codigo VARCHAR(30),
    @observaciones NVARCHAR(255) = NULL,
    @dispositivo NVARCHAR(100) = NULL,
    @ip_registro VARCHAR(45) = NULL,
    @latitud DECIMAL(10, 6) = NULL,
    @longitud DECIMAL(10, 6) = NULL,
    @evento_id INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM jornada WHERE jornada_id = @jornada_id AND estado = 'abierta')
    BEGIN
        RAISERROR('Jornada no existe o está cerrada.', 16, 1);
        RETURN;
    END

    DECLARE @tipo_evento_id INT;
    SELECT @tipo_evento_id = tipo_evento_id
      FROM tipo_evento
     WHERE codigo = @tipo_evento_codigo AND estado = 1;

    IF @tipo_evento_id IS NULL
    BEGIN
        RAISERROR('Tipo de evento no válido.', 16, 1);
        RETURN;
    END

    -- Evitar duplicados de evento consecutivo (opcional, puedes comentar si no lo necesitas)
    DECLARE @ultimo_evento INT;
    SELECT TOP 1 @ultimo_evento = tipo_evento_id
    FROM evento
    WHERE jornada_id = @jornada_id
    ORDER BY evento_id DESC;

    IF @ultimo_evento = @tipo_evento_id
    BEGIN
        RAISERROR('No puedes registrar el mismo evento consecutivamente.', 16, 1);
        RETURN;
    END

    INSERT INTO evento (
        jornada_id, tipo_evento_id, observaciones, dispositivo, ip_registro, latitud, longitud, hora_registro
    )
    VALUES (
        @jornada_id, @tipo_evento_id, @observaciones, @dispositivo, @ip_registro, @latitud, @longitud, CAST(GETDATE() AS TIME)
    );

    SET @evento_id = SCOPE_IDENTITY();
    SELECT 'OK' AS estado, @evento_id AS evento_id;
END
GO

-- ===========================================
-- SP: Cerrar Jornada
-- ===========================================
/*
Cierra una jornada (registra hora de fin y cambia estado a 'cerrada').
Solo si la jornada está abierta.
*/
CREATE PROCEDURE sp_cerrar_jornada
    @jornada_id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM jornada WHERE jornada_id = @jornada_id AND estado = 'abierta')
    BEGIN
        RAISERROR('Jornada no existe o ya está cerrada.', 16, 1);
        RETURN;
    END

    UPDATE jornada
    SET hora_fin = CAST(GETDATE() AS TIME),
        estado = 'cerrada',
        actualizado_en = GETDATE()
    WHERE jornada_id = @jornada_id;

    SELECT 'OK' AS estado, @jornada_id AS jornada_id;
END
GO

-- ===========================================
-- SP: Timeline de un Usuario
-- ===========================================
/*
Devuelve los eventos de un usuario en una fecha (orden cronológico),
con tipo de evento, observaciones y hora.
Ideal para mostrar el timeline del día.
*/
CREATE PROCEDURE sp_timeline_usuario
    @usuario_id INT,
    @fecha DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.evento_id,
        te.codigo AS tipo_evento,
        te.descripcion,
        e.observaciones,
        e.hora_registro,
        e.creado_en
    FROM evento e
    INNER JOIN tipo_evento te ON e.tipo_evento_id = te.tipo_evento_id
    INNER JOIN jornada j ON e.jornada_id = j.jornada_id
    WHERE j.usuario_id = @usuario_id AND j.fecha = @fecha
    ORDER BY e.hora_registro;
END
GO

-- ===========================================
-- SP: Jornadas Abiertas por Usuario
-- ===========================================
/*
Devuelve las jornadas abiertas (no cerradas) de un usuario.
Sirve para evitar que un usuario tenga dos jornadas abiertas.
*/
CREATE PROCEDURE sp_jornadas_abiertas_usuario
    @usuario_id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        j.jornada_id,
        j.fecha,
        j.tipo_jornada,
        j.hora_inicio,
        j.estado
    FROM jornada j
    WHERE j.usuario_id = @usuario_id AND j.estado = 'abierta'
    ORDER BY j.fecha DESC;
END
GO

-- ===========================================
-- FIN DEL SCRIPT
-- ===========================================

