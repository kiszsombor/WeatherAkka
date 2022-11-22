use WeatherTest;
go

create or alter proc InsertCurrentWeatherAndWeatherForecast
	@cityName NVARCHAR(100),
	@jsonVariable VARCHAR(MAX)
as
	DECLARE
		@cityExists int;

	DROP TABLE IF EXISTS #tmp_timeArray;
	DROP TABLE IF EXISTS #tmp_temperatureArray;

	SELECT @cityExists = COUNT(*) FROM CurrentWeather WHERE cityname = @cityName;
	if @cityExists > 0 
		begin
			DROP TABLE IF EXISTS #tmp_timeArray;
			DROP TABLE IF EXISTS #tmp_temperatureArray;

			UPDATE CurrentWeather
			SET temperature = n.temperature, windspeed = n.windspeed, winddirection = n.winddirection, weathercode = n.weathercode, time = CAST(REPLACE(n.time, 'T', ' ') AS DATETIME)
			FROM OPENJSON(@jsonVariable)
			  WITH (
				temperature FLOAT '$.current_weather.temperature',
				windspeed FLOAT '$.current_weather.windspeed',
				winddirection FLOAT '$.current_weather.winddirection',
				weathercode INT '$.current_weather.weathercode',
				time VARCHAR(50) '$.current_weather.time'
			  ) AS n
			WHERE cityname = @cityName;

			SELECT temperature_2m, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) as row_number
			INTO #tmp_temperatureArray2
			FROM OPENJSON(@jsonVariable)
				WITH 
				(
					temperature_2ms NVARCHAR(MAX) '$.hourly.temperature_2m' AS JSON
				)
			OUTER APPLY OPENJSON(temperature_2ms)
				 WITH (temperature_2m FLOAT '$')


			SELECT time, ROW_NUMBER() OVER(ORDER BY time) as row_number
			INTO #tmp_timeArray2
			FROM OPENJSON(@jsonVariable)
				WITH 
				(
					times NVARCHAR(MAX) '$.hourly.time' AS JSON
				)
			OUTER APPLY OPENJSON(times)
				 WITH (time varchar(50) '$');

			-- UPDATE WeatherForecast
			-- SET time = CAST(REPLACE(a2.time, 'T', ' ') AS DATETIME), temperature_2m = a1.temperature_2m 
			-- FROM #tmp_temperatureArray2 a1 
			-- 	JOIN #tmp_timeArray2 a2 ON a1.row_number = a2.row_number
			-- WHERE cityname = @cityname;

			

			BEGIN TRY  
				DELETE FROM WeatherForecast
				WHERE cityname = @cityName; 
			END TRY
			BEGIN CATCH  
				SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
				RAISERROR('DELETE ERROR!', 16, 1); 
			END CATCH; 

			INSERT WeatherForecast(cityname, time, temperature_2m)
			SELECT @cityName, CAST(REPLACE(a2.time, 'T', ' ') AS DATETIME), a1.temperature_2m 
			FROM #tmp_temperatureArray2 a1 
				JOIN #tmp_timeArray2 a2 ON a1.row_number = a2.row_number;

			return; 
		end;

	INSERT CurrentWeather(cityname, temperature, windspeed, winddirection, weathercode, time)
	SELECT @cityName, temperature, windspeed, winddirection, weathercode, CAST(REPLACE(time, 'T', ' ') AS DATETIME)
	FROM OPENJSON(@jsonVariable)
	  WITH (
		temperature FLOAT '$.current_weather.temperature',
		windspeed FLOAT '$.current_weather.windspeed',
		winddirection FLOAT '$.current_weather.winddirection',
		weathercode INT '$.current_weather.weathercode',
		time VARCHAR(50) '$.current_weather.time'
	  );

	SELECT temperature_2m, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) as row_number
	INTO #tmp_temperatureArray
	FROM OPENJSON(@jsonVariable)
		WITH 
		(
			temperature_2ms NVARCHAR(MAX) '$.hourly.temperature_2m' AS JSON
		)
	OUTER APPLY OPENJSON(temperature_2ms)
		 WITH (temperature_2m FLOAT '$')


	SELECT time, ROW_NUMBER() OVER(ORDER BY time) as row_number
	INTO #tmp_timeArray
	FROM OPENJSON(@jsonVariable)
		WITH 
		(
			times NVARCHAR(MAX) '$.hourly.time' AS JSON
		)
	OUTER APPLY OPENJSON(times)
		 WITH (time varchar(50) '$');

	INSERT WeatherForecast(cityname, time, temperature_2m)
	SELECT @cityName, CAST(REPLACE(a2.time, 'T', ' ') AS DATETIME), a1.temperature_2m 
	FROM #tmp_temperatureArray a1 
		JOIN #tmp_timeArray a2 ON a1.row_number = a2.row_number;