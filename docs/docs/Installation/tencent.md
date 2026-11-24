# Use Tencent Cloud COS

You can store packages to [Tencent Cloud  Object Storage Service](https://cloud.tencent.com/document/product/436/6222).

## Configure BaGest

You can modify BaGest's configurations by editing the `appsettings.json` file. For the full list of configurations, please refer to [BaGest's configuration](../configuration.md) guide.

### Tencent Cloud Object Storage Service

Update the `appsettings.json` file:

```json
{
    ...

    "Storage": {
        "Type": "TencentCos",
        "AppId": "",
        "SecretId": "",
        "SecretKey": "",
        "BucketName": "bagest",
        "Region": "ap-guangzhou"        
    },

    ...
}
```
