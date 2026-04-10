USE
DeviceManagerDb;
GO

IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
INSERT INTO Users (Name, Role, Location)
VALUES ('Alice Johnson', 'Developer', 'London'),
       ('Bob Smith', 'QA Engineer', 'Berlin'),
       ('Carol White', 'Designer', 'Paris');
END
GO

IF NOT EXISTS (SELECT 1 FROM Devices)
BEGIN
INSERT INTO Devices (Name, Manufacturer, Type, OperatingSystem, OsVersion, Processor, RamAmount, Description,
                     AssignedUserId)
VALUES ('iPhone 15 Pro', 'Apple', 'phone', 'iOS', '17.4', 'A17 Pro', 8, 'Apple flagship phone', 1),
       ('Galaxy S24', 'Samsung', 'phone', 'Android', '14.0', 'Snapdragon 8 Gen 3', 12, 'Samsung flagship phone', 2),
       ('iPad Pro 12.9', 'Apple', 'tablet', 'iPadOS', '17.4', 'M2', 16, 'Apple pro tablet', NULL),
       ('Pixel 8', 'Google', 'phone', 'Android', '14.0', 'Tensor G3', 8, 'Google flagship phone', 3),
       ('Galaxy Tab S9', 'Samsung', 'tablet', 'Android', '14.0', 'Snapdragon 8 Gen 2', 12, 'Samsung pro tablet', NULL);
END
GO