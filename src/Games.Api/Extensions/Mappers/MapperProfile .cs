using AutoMapper;
using Core.PosTech8Nett.Api.Domain.Model.Game;
using Games.Domain.Entities;

namespace Games.Api.Extensions.Mappers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Game, GameResponse>();

            CreateMap<GameRequest, Game>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Genres, opt => opt.Ignore()); // será tratado manualmente
        }
    }

}
