-- =============================================================================
-- KYOKA GAME DATABASE SCHEMA (PostgreSQL 14+)
-- Complete backend architecture for user authentication, persistent player data,
-- matchmaking, inventory, transactions, and anti-exploit audit trails.
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. USERS & ACCOUNTS
CREATE TABLE IF NOT EXISTS users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(32) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP WITH TIME ZONE,
    is_banned BOOLEAN DEFAULT FALSE,
    ban_reason TEXT
);

-- 2. PLAYER PROFILES
CREATE TABLE IF NOT EXISTS player_profiles (
    profile_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    display_name VARCHAR(32) NOT NULL,
    player_level INT NOT NULL DEFAULT 1,
    current_xp INT NOT NULL DEFAULT 0,
    kyoka_credits INT NOT NULL DEFAULT 500,
    aether_shards INT NOT NULL DEFAULT 10,
    title VARCHAR(64) DEFAULT 'Aether Initiate',
    battle_royale_wins INT DEFAULT 0,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 3. CHARACTER CUSTOMIZATION
CREATE TABLE IF NOT EXISTS character_customization (
    customization_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    hair_style INT DEFAULT 0,
    hair_color_r REAL DEFAULT 0.2,
    hair_color_g REAL DEFAULT 0.6,
    hair_color_b REAL DEFAULT 0.9,
    eye_color_r REAL DEFAULT 0.1,
    eye_color_g REAL DEFAULT 0.8,
    eye_color_b REAL DEFAULT 0.7,
    outfit_color_r REAL DEFAULT 0.15,
    outfit_color_g REAL DEFAULT 0.2,
    outfit_color_b REAL DEFAULT 0.35,
    skin_tone_r REAL DEFAULT 0.95,
    skin_tone_g REAL DEFAULT 0.82,
    skin_tone_b REAL DEFAULT 0.72
);

-- 4. INVENTORY ITEMS
CREATE TABLE IF NOT EXISTS player_inventory (
    item_entry_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    item_id VARCHAR(64) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    acquired_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_user_item UNIQUE (user_id, item_id)
);

-- 5. MISSION PROGRESS
CREATE TABLE IF NOT EXISTS player_missions (
    mission_progress_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    mission_id VARCHAR(64) NOT NULL,
    current_progress INT NOT NULL DEFAULT 0,
    is_completed BOOLEAN DEFAULT FALSE,
    completed_at TIMESTAMP WITH TIME ZONE,
    CONSTRAINT uq_user_mission UNIQUE (user_id, mission_id)
);

-- 6. MATCH HISTORY & RESULTS
CREATE TABLE IF NOT EXISTS match_records (
    match_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    game_mode VARCHAR(32) NOT NULL,
    started_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP WITH TIME ZONE,
    winner_user_id UUID REFERENCES users(user_id)
);

CREATE TABLE IF NOT EXISTS match_participants (
    participant_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    match_id UUID NOT NULL REFERENCES match_records(match_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    final_rank INT,
    eliminations INT DEFAULT 0,
    survival_seconds INT DEFAULT 0,
    credits_earned INT DEFAULT 0,
    shards_earned INT DEFAULT 0
);

-- 7. AUDIT TRAIL / ANTI-EXPLOIT TRANSACTIONS
CREATE TABLE IF NOT EXISTS financial_transactions (
    transaction_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    currency_type VARCHAR(16) NOT NULL, -- 'CREDITS' or 'SHARDS'
    delta_amount INT NOT NULL,
    balance_after INT NOT NULL,
    source_tag VARCHAR(64) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- INDEXES FOR PERFORMANCE
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_profiles_user ON player_profiles(user_id);
CREATE INDEX IF NOT EXISTS idx_inventory_user ON player_inventory(user_id);
CREATE INDEX IF NOT EXISTS idx_transactions_user ON financial_transactions(user_id);
