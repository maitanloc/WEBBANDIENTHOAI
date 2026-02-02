-- Update LaptopConfigurations table to increase field sizes
ALTER TABLE LaptopConfigurations
ALTER COLUMN ScreenSize NVARCHAR(100) NULL;

ALTER TABLE LaptopConfigurations
ALTER COLUMN Color NVARCHAR(100) NULL;

ALTER TABLE LaptopConfigurations
ALTER COLUMN Weight NVARCHAR(100) NULL;
