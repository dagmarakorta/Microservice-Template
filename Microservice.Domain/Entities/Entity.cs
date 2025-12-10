namespace Microservice.Domain.Entities
{
    public class Entity
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        private Entity(Guid id, string name, string? description, bool isActive, DateTimeOffset createdAt, DateTimeOffset? updatedAt)
        {
            Id = id;
            Name = name;
            Description = description;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public static Entity Create(string name, string? description = null)
        {
            var now = DateTimeOffset.UtcNow;

            return new Entity(Guid.NewGuid(), name, description?.Trim(), true, now, now);
        }

        public void UpdateDetails(string name, string? description)
        {
            Name = name;
            Description = description;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
