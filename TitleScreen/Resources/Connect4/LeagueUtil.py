import Agent
import random as rand
from collections import defaultdict

class LeagueUtil:
	def __init__(self, board, league):
		self.player_names = []
		self.board_agents = []
		self.league_agents = []
		
		#pre-compute the league Q table to be able to send into functions later without calculating it again.
		#significantly reduces load time
		LeagueQ = ''
		with open('\Resources\Connect4\league.txt', 'r') as f:
			for i in f.readlines():
				LeagueQ = i
		LeagueQ = eval(LeagueQ)
		LeagueQ = defaultdict(lambda: 0.0, LeagueQ)

		#pre-compute the available board and league agents. significantly reduces load time
		MaxTactics = Agent.Agent(board, self.select_difficulty(), 'max')
		MaxStrategy = Agent.Agent(league, 'league', 'max', LeagueQ)
		RandomTactics = Agent.Agent(board, self.select_difficulty(), 'random')
		RandomStrategy = Agent.Agent(league, 'league', 'random', LeagueQ)
		
		#store all available agents to be randomly chosen later during league play
		self.player_names.append('learning strategy and tactics')
		self.board_agents.append(MaxTactics)
		self.league_agents.append(MaxStrategy)

		self.player_names.append('learning tactics only')
		self.board_agents.append(MaxTactics)
		self.league_agents.append(RandomStrategy)

		self.player_names.append('learning strategy only')
		self.board_agents.append(RandomTactics)
		self.league_agents.append(MaxStrategy)

		self.player_names.append('no learning')
		self.board_agents.append(RandomTactics)
		self.league_agents.append(RandomStrategy)

	def get_names(self):
		return self.player_names
		
	def get_boards(self):
		return self.board_agents
		
	def get_leagues(self):
		return self.league_agents
		
	#randomly select the difficulty of board agents
	def select_difficulty(self):
		diffdict = {1 : r'\Resources\Connect4\easy.txt',
                2 : r'\Resources\Connect4\medium.txt',
                3 : r'\Resources\Connect4\hard.txt'}
		return diffdict[rand.randint(1,3)]
		
	def get_board_agent(self, index):
		return self.board_agents[index]
		
	def get_league_agent(self, index):
		return self.league_agents[index]