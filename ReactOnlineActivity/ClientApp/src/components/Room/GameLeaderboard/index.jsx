import React, { Component } from 'react';
import authorizeFetch from '../../../utils/authorizeFetch';
import { RoomHubEvents } from '../RoomConstants';
import styles from './gameLeaderboard.module.css';

export default class GameLeaderboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            players: [],
            explainingPlayerName: null,
            loadingPlayers: true
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_PLAYER, (player) => {
            this.setState({
                players: [...this.state.players, player]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.LEAVE_ROOM, (name) => {
            this.setState({
                players: this.state.players.filter(p => p.name !== name)
            });
        });

        this.props.hubConnection.on(RoomHubEvents.ROUND_INFO, async (explainingPlayerName) => {
            this.setState({
                explainingPlayerName
            });
            await this.fetchPlayers();
        });
        
        this.props.hubConnection.on(RoomHubEvents.GAME_OVER, async () => {
            await this.fetchPlayers();
            
            setTimeout(async () => {
                await this.fetchPlayers();
                await this.props.hubConnection.invoke(RoomHubEvents.REQUEST_ROUND, this.props.roomId);
            }, 10000);
        });
        
        await this.fetchPlayers();
        await this.props.hubConnection.invoke(RoomHubEvents.REQUEST_ROUND, this.props.roomId);
    }

    fetchPlayers = async () => {
        const players = await authorizeFetch(`/api/rooms/${this.props.roomId}/players`);
        this.setState({
            loadingPlayers: false,
            players
        });
    };

    renderPlayers = () => {
        return [].concat(this.state.players)
            .sort(p => -p.score)
            .map((player, index) =>
                <div className={styles.player} key={index}>
                    <div>
                        {player.photoUrl && <img className={styles.player__image} src={player.photoUrl} alt={player.name}/>}
                        <span
                            className={`${styles.player__name} ${this.state.explainingPlayerName === player.name ? styles.explaining : ''}`}
                        >
                        {player.name}
                    </span>
                    </div>
                    <span className={styles.player__score}>{player.score}</span>
                </div>
            );
    }

    render() {
        return (
            <section className={styles.board}>
                <div className={styles.players}>
                    {(this.state.loadingPlayers)
                        ? <div className={styles.loading}>
                            <p>Загрузка игроков...</p>
                        </div>
                        : this.renderPlayers()
                    }
                </div>
            </section>
        );
    }
}
