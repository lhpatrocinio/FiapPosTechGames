using AutoMapper;
using Core.PosTech8Nett.Api.Domain.Model.Game;
using Games.Api.Dtos.Requests;
using Games.Api.Dtos.Responses;
using Games.Domain.Entities;
using Games.Infrastructure.Search.Interfaces;
using Games.Infrastructure.Search.Models;

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

            CreateMap<GameSearchRequestDto, GameSearchRequest>();
            
            CreateMap<SearchResult<GameDocument>, SearchResultResponse>()
                .ForMember(dest => dest.Games, opt => opt.MapFrom(src => src.Documents));
            
            CreateMap<GameDocument, GameDocumentResponse>();
            
            // Analytics mappings
            CreateMap<PopularGamesResult, AnalyticsPopularGamesResponse>();
            CreateMap<PopularGameItem, PopularGameDto>();
            CreateMap<GenreStatsResult, AnalyticsGenreStatsResponse>();
            CreateMap<GenreStatItem, GenreStatsDto>();
            CreateMap<PriceRangeStatsResult, AnalyticsPriceRangeResponse>();
            CreateMap<PriceRangeItem, PriceRangeDto>();
            CreateMap<TopRatedGamesResult, AnalyticsTopRatedResponse>();
            CreateMap<TopRatedGameItem, TopRatedGameDto>();
            CreateMap<CatalogOverviewResult, AnalyticsCatalogOverviewResponse>();
            
            // Recommendation mappings
            CreateMap<GameRecommendationResult, GameRecommendationResponseDto>();
            CreateMap<RecommendedGame, RecommendedGameDto>();
            CreateMap<SimilarGamesResult, SimilarGamesResponseDto>();
            CreateMap<SimilarGame, SimilarGameDto>();
            CreateMap<GenreBasedRecommendationResult, GenreBasedRecommendationResponseDto>();
            CreateMap<GenreRecommendation, GenreRecommendationDto>();
            CreateMap<UserPreferencesRecommendationResult, UserPreferencesRecommendationResponseDto>();
            CreateMap<UserPreferencesProfile, UserPreferencesProfileDto>();
        }
    }
}
