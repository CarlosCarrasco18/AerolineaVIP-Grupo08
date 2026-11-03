USE AerolineaVIP;
GO

DECLARE @Inicio DATE = CAST(GETDATE() AS DATE);
DECLARE @Fin DATE = DATEADD(DAY, 13, @Inicio);
DECLARE @AV1 INT = (SELECT AeronaveId FROM dbo.Aeronave WHERE Matricula = 'AV-01');
DECLARE @AV2 INT = (SELECT AeronaveId FROM dbo.Aeronave WHERE Matricula = 'AV-02');
DECLARE @AV3 INT = (SELECT AeronaveId FROM dbo.Aeronave WHERE Matricula = 'AV-03');

EXEC dbo.usp_GenerarVuelos @FechaInicio = @Inicio, @FechaFin = @Fin;

UPDATE v
   SET v.AeronaveId = CASE WHEN f.DiaSemana IN (1, 3, 5) THEN @AV1
                           WHEN f.DiaSemana IN (2, 4, 6) THEN @AV2
                           ELSE @AV3 END
  FROM dbo.Vuelo v
  JOIN dbo.Frecuencia f ON f.FrecuenciaId = v.FrecuenciaId
 WHERE v.AeronaveId IS NULL
   AND v.EstadoVueloCodigo = 'PROGRAMADO'
   AND v.FechaOperacion BETWEEN @Inicio AND @Fin
   AND NOT (v.FechaOperacion = '20261107' AND f.HoraSalida = '19:00');

INSERT INTO dbo.Itinerario (Referencia, Nombre, EstadoItinerarioCodigo)
SELECT 'IT-' + CONVERT(CHAR(8), d.FechaOperacion, 112),
       CASE WHEN d.DiaSemana IN (1, 3, 5) THEN N'Quito – Manta – Quito'
            WHEN d.DiaSemana IN (2, 4, 6) THEN N'Quito – Guayaquil – Quito'
            ELSE N'Quito – Galápagos – Quito (escala en Guayaquil)' END
         + N' · ' + CONVERT(NVARCHAR(10), d.FechaOperacion, 103),
       'ACTIVO'
  FROM (SELECT DISTINCT FechaOperacion, DiaSemana FROM dbo.vw_VueloDetalle WHERE FechaOperacion BETWEEN @Inicio AND @Fin) d
 WHERE NOT EXISTS (SELECT 1 FROM dbo.Itinerario i WHERE i.Referencia = 'IT-' + CONVERT(CHAR(8), d.FechaOperacion, 112));

INSERT INTO dbo.TramoItinerario (ItinerarioId, VueloId, Orden)
SELECT i.ItinerarioId,
       vd.VueloId,
       ROW_NUMBER() OVER (PARTITION BY vd.FechaOperacion ORDER BY vd.HoraSalida)
  FROM dbo.vw_VueloDetalle vd
  JOIN dbo.Itinerario i ON i.Referencia = 'IT-' + CONVERT(CHAR(8), vd.FechaOperacion, 112)
 WHERE vd.FechaOperacion BETWEEN @Inicio AND @Fin
   AND (vd.DiaSemana = 7 OR vd.HoraSalida IN ('09:00', '14:00'))
   AND NOT EXISTS (SELECT 1 FROM dbo.TramoItinerario t WHERE t.ItinerarioId = i.ItinerarioId);
GO
