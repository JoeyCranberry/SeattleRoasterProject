using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Models;

public class RoasterModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public int RoasterId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RoasterLocation Location { get; set; }
    public string ShopURL { get; set; }
    public string ImageURL { get; set; }
    public string ImageClass { get; set; }
    public int FoundingYear { get; set; }
    public bool IsExcluded { get; set; } = false;
    public bool ContactedForPermission { get; set; } = false;
    public bool RecievedPermission { get; set; } = false;
    public bool HasParser { get; set; } = false;
}