namespace DeviceManager.IntegrationTests;

[CollectionDefinition("DevicesCollection")]
public class DevicesCollection : ICollectionFixture<DeviceManagerWebAppFactory> { }

[CollectionDefinition("UsersCollection")]
public class UsersCollection : ICollectionFixture<DeviceManagerWebAppFactory> { }