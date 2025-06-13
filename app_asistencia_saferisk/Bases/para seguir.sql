INSERT INTO jornada (usuario_id, tipo_jornada,fecha, hora_inicio, estado, creado_en)
VALUES (20, 'oficina','2025-06-09', '07:57:00.8333333', 'cerrado', '2025-06-09 07:57:30.833')

INSERT INTO evento(jornada_id,tipo_evento_id,observaciones,dispositivo,ip_registro,latitud,longitud,creado_en,hora_registro)
VALUES(93,1,'remoto','Web','157.100.87.134','-0.147904','-78.490547','2025-06-09 07:57:00.437','07:57:00.8333333')

INSERT INTO evento(jornada_id,tipo_evento_id,observaciones,dispositivo,ip_registro,latitud,longitud,creado_en,hora_registro)
VALUES(93,3,'oficina','Web','149.50.206.131','-0.147904','-78.490547','2025-06-09 13:57:00.437','13:57:00.8333333')

INSERT INTO evento(jornada_id,tipo_evento_id,observaciones,dispositivo,ip_registro,latitud,longitud,creado_en,hora_registro)
VALUES(93,4,'oficina','Web','149.50.206.131','-0.147904','-78.490547','2025-06-09 14:28:00.833','14:28:00.8333333')

INSERT INTO evento(jornada_id,tipo_evento_id,observaciones,dispositivo,ip_registro,latitud,longitud,creado_en,hora_registro)
VALUES(93,2,'remoto','Web','157.100.140.78','-0.147904','-7.490547','2025-06-09 17:11:00.833','17:11:32.8333333')

update jornada set hora_fin = '17:11:00.8333333'  where jornada_id = 93
update jornada set actualizado_en = '2025-06-09 17:11:30.833'  where jornada_id = 93

select * from usuario

select * from jornada where usuario_id = 17

select * from evento where jornada_id = 84

select * from tipo_evento

delete evento where evento_id = 303


update jornada set fecha = '2025-06-07' where jornada_id = 88
