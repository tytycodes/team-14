import random as rand
class LeagueEnvironment:
	def __init__(self, board_environment):
		self.board = board_environment  
	#set all available players for league play, given by the league utility functions
	def set_players(self, player_names, league_agents, board_agents):
		self.player_names = player_names
		self.league_agents = league_agents
		self.board_agents = board_agents
		assert(len(player_names) == len(league_agents) == len(board_agents) )
		self.num_players = len(player_names)
	#choose a new pair to play for the game
	def reset_pair(self):
		# randomly select 2 players
		player_indices = list(range(self.num_players))
		self.Ai = rand.choice(player_indices)
		self.first = self.board.reset()
		self.league_agents[self.Ai].reset_past()
		self.A_wins = 0;
		self.A_chips=100;
		self.Player_wins = 0;
		self.Player_chips=100;
		self.ties = 0;
		self.chip_mul=1
		self.min_bid=5
		self.game_counter=1
		#return the AI index and who will go first
		return [str(self.Ai), str(self.first)]
	#return the current state of the league from the perspective of the AI
	def get_state(self):
		return (self.A_chips,self.A_wins,self.ties,self.Player_chips,self.Player_wins,self.player_names[self.Ai],'learning strategy and tactics')
	#return number of games the current pair has played
	def pair_games_played(self):
		return self.A_wins + self.ties + self.B_wins
	#return the actions available to the current player depending on if they are choosing first at the league level or second
	def available_actions(self, first):
		if first:
			return ['quit','single bet','double bet','triple bet']
		else:
			return ['quit','call']