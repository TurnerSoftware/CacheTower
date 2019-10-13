using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	public class ReplaceEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private TEntity Entity { get; }

		public ReplaceEntityCommand(TEntity entity)
		{
			Entity = entity;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
			yield return new ReplaceOneModel<TEntity>(definition.CreateIdFilterFromEntity(Entity), Entity);
		}
	}
}
