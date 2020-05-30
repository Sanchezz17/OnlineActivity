import React, { Component } from 'react';
import { Stage, Layer, Line } from 'react-konva';
import authorizeFetch from '../../../utils/authorizeFetch';
import Palette from '../Palette';
import { RoomHubEvents } from '../RoomConstants';
import styles from './field.module.css';
import Timer from '../Timer';

export default class Field extends Component {
    isDrawing = false;
    stageRef = null;
    defaultState = {
        lines: [],
        isLoadingField: true,
        isActiveUser: false,
        activeUser: null,
        hiddenWord: null,
        drawingColor: '#000',
        isPalletShow: false,
        explainingPlayerName: null
    };
    
    constructor(props) {
        super(props);
        this.state = {
            ...this.defaultState
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_LINE, (line) => {
            this.setState({
                lines: [...this.state.lines, line]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.CLEAR_FIELD, () => {
            this.setState({
                lines: []
            });
        });

        this.props.hubConnection.on(RoomHubEvents.ROUND_INFO, (explainingPlayerName) => {
            const isActiveUser = explainingPlayerName === this.props.user.name;
            if (isActiveUser) {
                this.props.hubConnection.invoke(RoomHubEvents.REQUEST_WORD, this.props.roomId);
            }
            this.setState({ isActiveUser, explainingPlayerName });
            this.fetchLines();
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_HIDDEN_WORD, (hiddenWord) => {
            this.setState({ hiddenWord });
        });

        this.props.hubConnection.on(RoomHubEvents.GAME_OVER, () => {

            setTimeout(async () => {
                this.setState({ ...this.defaultState });
                
                await this.fetchLines();
            }, 10000);
        });

        await this.fetchLines();
    }

    fetchLines = async () => {
        const lines = await authorizeFetch(`/api/fields/${this.props.roomId}`);
        this.setState({
            isLoadingField: false,
            lines: lines || []
        });
    };

    addLine = () => {
        this.props.hubConnection.invoke(RoomHubEvents.NEW_LINE,
            this.props.roomId, this.state.lines[this.state.lines.length - 1]);
    };

    getNewEmptyLine = () => {
        return {
            color: this.state.drawingColor,
            coordinates: []
        };
    };

    handleMouseDown = () => {
        this.isDrawing = true;
        this.setState({
            lines: [
                ...this.state.lines,
                this.getNewEmptyLine()
            ]
        });
    };

    handleMouseUp = () => {
        this.isDrawing = false;
        this.addLine();
    };

    handleMouseMove = () => {
        if (!this.isDrawing) {
            return;
        }
        const stage = this.stageRef.getStage();
        const point = stage.getPointerPosition();
        const { lines } = this.state;
        let lastLine = lines[lines.length - 1] || this.getNewEmptyLine();
        lastLine.coordinates = lastLine.coordinates.concat([point.x, point.y]) || [];
        lines.splice(lines.length - 1, 1, lastLine);
        this.setState({
            lines: lines.concat()
        });
    };

    clearField = () => {
        this.props.hubConnection.invoke(RoomHubEvents.CLEAR_FIELD, this.props.roomId);
        this.setState({ lines: [] });
    };

    giveUp = () => {
        this.props.hubConnection.invoke(RoomHubEvents.GIVE_UP, this.props.roomId, this.props.user.name);
    };
    
    changeDrawingColor = (drawingColor) => {
        this.setState({ drawingColor });
    };

    onEraserClick = () => {
        this.setState({ drawingColor: '#fff' });
    };

    render() {
        if (typeof window === 'undefined') {
            return null;
        }

        return (
            <section className={styles.field} id='field'>
                {this.state.explainingPlayerName && <span className={`btn btn-warning btn-sm ${styles.timerContainer}`}>
                    Оставшееся время: <Timer roomId={this.props.roomId} hubConnection={this.props.hubConnection}/>
                </span>}
                {this.state.isActiveUser ?
                    <>
                        <section className={styles.drawerTools}>
                            <Palette changeColor={this.changeDrawingColor}/>
                            <button className={styles.eraser}
                                    onClick={this.onEraserClick}/>
                            <button
                                className={`btn btn-dark btn-sm ${styles.clear}`}
                                onClick={this.clearField}
                            >
                                Очистить
                            </button>
                            <button
                                className={`btn btn-primary btn-sm ${styles.end}`}
                                onClick={this.giveUp}
                            >
                                Сдаться
                            </button>
                        </section>
                        <div className={styles.wordContainer}>
                            <span
                                className={`btn btn-warning btn-sm ${styles.word}`}
                            >
                                {`Загаданное слово: ${this.state.hiddenWord}`}
                            </span>
                        </div>
                    </> : ''
                }
                {this.state.isLoadingField
                    ? <div className={styles.loading}>
                        <p>Загрузка...</p>
                    </div>
                    : <Stage
                        width={document.getElementById('field').offsetWidth - 3}
                        height=
                            {document.getElementById('field').offsetHeight -
                            (this.state.isActiveUser ? 65 : 0)}
                        onContentMousedown={this.state.isActiveUser ? this.handleMouseDown : () => {
                        }}
                        onContentMousemove={this.state.isActiveUser ? this.handleMouseMove : () => {
                        }}
                        onContentMouseup={this.state.isActiveUser ? this.handleMouseUp : () => {
                        }}
                        ref={node => {
                            this.stageRef = node;
                        }}>
                        <Layer>
                            {this.state.lines.map((line, i) => (
                                <Line key={i} points={line.coordinates}
                                      stroke={line.color}
                                      strokeWidth={line.color === '#fff' ? 20 : 3}
                                />
                            ))}
                        </Layer>
                    </Stage>
                }
            </section>
        );
    }
}
