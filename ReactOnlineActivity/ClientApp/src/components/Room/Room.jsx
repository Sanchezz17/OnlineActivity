import React, {Component} from 'react';
import * as signalR from "@aspnet/signalr";
import Leaderboard from './Leaderboard/Leaderboard';
import Field from './Field/Field';
import Chat from './Chat/Chat';
import {RoomHubEvents} from './RoomConstants';
import {CopyToClipboard} from "react-copy-to-clipboard";
import styles from './room.module.css';

export default class Room extends Component {
    constructor(props) {
        super(props);
        this.roomId = this.props.match.params.roomId;
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/room")
            .build();

        this.state = {
            loading: true
        }
    }

    async componentDidMount() {
        await this.hubConnection.start()
        const {user} = this.props;
        const response = await fetch(`/api/rooms/${this.roomId}/join?userName=${user.name}`);
        const joinRoomDto = await response.json();
        console.log(joinRoomDto)
        this.hubConnection.invoke(RoomHubEvents.JOIN_ROOM, this.roomId, user.name, joinRoomDto.alreadyInRoom);
        if (!joinRoomDto.alreadyInRoom) {
            this.hubConnection.invoke(RoomHubEvents.NEW_PLAYER, this.roomId, joinRoomDto.player);
        }
        const roomResponse = await fetch(`/api/rooms/${this.roomId}`)
        this.room = await roomResponse.json();
        this.setState({loading: false})
    }

    componentWillUnmount() {
        const { user } = this.props;
        this.hubConnection.invoke(RoomHubEvents.LEAVE_ROOM, this.roomId, user.name);
        console.log("leave room")
        fetch(`/api/leave?roomId=${this.roomId}&playerName=${user.name}`)
    }

    render() {
        const {user} = this.props;

        return (
            this.state.loading
                ? <div className={styles.loading}>
                    <p>Загрузка игры...</p>
                </div>
                :  <div>
                    <div className={styles.nameContainer}>
                        <span className={styles.name}>{this.room.settings.name}</span>
                        <CopyToClipboard text={`${window.location.host}/rooms/${this.roomId}`}>
                            <span className={styles.copy}>Скопировать ссылку на комнату</span>
                        </CopyToClipboard>
                    </div>
                    <p>{this.room.settings.description}</p>
                    <section className={styles.room}>
                        <Leaderboard roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                        <Field roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                        <Chat roomId={this.roomId} user={user} hubConnection={this.hubConnection}/>
                    </section>
                </div>
        );
    }
}