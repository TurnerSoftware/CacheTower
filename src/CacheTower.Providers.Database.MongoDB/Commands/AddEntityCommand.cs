using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;

namespace CacheTower.Providers.Database.MongoDB.Commands
{
	public class AddEntityCommand<TEntity> : IWriteCommand<TEntity> where TEntity : class
	{
		private TEntity Entity { get; }

		public AddEntityCommand(TEntity entity)
		{
			Entity = entity;
		}

		public IEnumerable<WriteModel<TEntity>> GetModel()
		{
			yield return new InsertOneModel<TEntity>(Entity);
		}
	}
}
