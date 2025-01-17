﻿using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace DayPlanner.FireStore;

public class FireStoreUserStore(FirebaseApp app, IMapper mapper) : IUserStore
{
    private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app) ?? throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");
    private readonly IMapper _mapper = mapper;

    public async Task<User> CreateAsync(RegisterUserRequest args)
    {
        ArgumentNullException.ThrowIfNull(args);
        UserRecord firebaseUser = await _firebaseAuth.CreateUserAsync(_mapper.Map<UserRecordArgs>(args));
        return _mapper.Map<User>(firebaseUser);
    }

    public async Task<User> GetByIdAsync(string uid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uid);

        UserRecord firebaseUser = await _firebaseAuth.GetUserAsync(uid);
        return _mapper.Map<User>(firebaseUser);
    }
}
